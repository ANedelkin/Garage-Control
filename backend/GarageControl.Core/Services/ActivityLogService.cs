using System.Text.Json;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.Enums;
using System.Text.Json.Serialization;
using GarageControl.Core.Services.Helpers;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using GarageControl.Shared.Constants;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GarageControl.Core.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IRepository _repository;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(IRepository repository, ILogger<ActivityLogService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ── Shared actor resolution ────────────────────────────────────────────

        private async Task<(string? actorId, string actorDisplayName)> ResolveActorInfoAsync(
            string userId)
        {
            var userExists = await _repository.GetAllAsNoTracking<User>()
                .AnyAsync(u => u.Id == userId);

            if (!userExists) return (null, "Unknown User");

            var worker = await _repository.GetAllAsNoTracking<Worker>()
                .Where(w => w.UserId == userId)
                .Select(w => new { w.Id, w.Name })
                .FirstOrDefaultAsync();

            string actorDisplayName = worker?.Name ?? "Unknown User";

            return (worker?.Id, actorDisplayName);
        }

        private async Task SaveLogAsync(
            string workshopId,
            string messageMarkup,
            string? logType = null,
            string? logData = null)
        {
            await _repository.AddAsync(new ActivityLog
            {
                MessageMarkup = messageMarkup,
                WorkshopId = workshopId,
                LogType = logType,
                LogData = logData,
            });

            await _repository.SaveChangesAsync();
        }

        // ── Public ────────────────────────────────────────────────

        public async Task LogActionAsync(
            string userId,
            string workshopId,
            LogEntityType logType,
            ActivityLogData logData)
        {
            var (actorId, actorName) = await ResolveActorInfoAsync(userId);

            logData = logData with { ActorId = actorId, ActorName = actorName };

            string messageMarkup = ActivityLogRenderer.BuildMessageMarkup(logType, logData);
            if (string.IsNullOrEmpty(messageMarkup)) return;

            string serialised = JsonSerializer.Serialize(logData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            await SaveLogAsync(workshopId, messageMarkup, logType.ToString(), serialised);
        }

        public async Task<(IEnumerable<ActivityLogVM> Logs, int TotalCount)> GetLogsAsync(string workshopId, int page = 0, DateTime? startDate = null, DateTime? endDate = null, string? search = null)
        {
            var query = _repository.GetAllAsNoTracking<ActivityLog>()
                .Where(l => l.WorkshopId == workshopId);

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                throw new ArgumentException("Start date cannot be after end date.");
            }

            if (startDate.HasValue)
            {
                // PostgreSQL 'timestamp with time zone' requires UTC offsets (00:00)
                var utcStart = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                query = query.Where(l => l.Timestamp >= utcStart);
            }

            if (endDate.HasValue)
            {
                // Ensure we include the entire day for the end date, forced to UTC
                var utcEnd = new DateTimeOffset(endDate.Value.Date, TimeSpan.Zero).AddDays(1).AddTicks(-1);
                query = query.Where(l => l.Timestamp <= utcEnd);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                // We fallback to searching in MessageMarkup or JSON Data
                query = query.Where(l => l.MessageMarkup.ToLower().Contains(lowerSearch) ||
                                        (l.LogData != null && l.LogData.ToLower().Contains(lowerSearch)));
            }

            var totalCount = await query.CountAsync();

            var skip = page * ActivityLogConstants.DefaultPageSize;
            var take = ActivityLogConstants.DefaultPageSize;

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            // 1. Deserialize and collect all referenced entities to pre-fetch existence
            var logDataMap = new Dictionary<ActivityLog, ActivityLogData>();
            var referenced = new HashSet<(LogEntityType Type, string Id)>();

            foreach (var log in logs)
            {
                if (!string.IsNullOrEmpty(log.LogType) && !string.IsNullOrEmpty(log.LogData))
                {
                    try
                    {
                        var data = JsonSerializer.Deserialize<ActivityLogData>(log.LogData!, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                        });
                        if (data != null)
                        {
                            logDataMap[log] = data;
                            if (Enum.TryParse<LogEntityType>(log.LogType, true, out var entityType))
                            {
                                foreach (var r in ActivityLogRenderer.GetReferencedEntities(entityType, data))
                                {
                                    referenced.Add(r);
                                }
                            }
                        }
                    }
                    catch { /* Skip malformed logs */ }
                }
            }

            // 2. Check existence in bulk to avoid N+1
            var existenceSet = new HashSet<(LogEntityType Type, string Id)>();
            var archivedSet = new HashSet<(LogEntityType Type, string Id)>();
            var byType = referenced.GroupBy(r => r.Type);

            foreach (var group in byType)
            {
                var ids = group.Select(r => r.Id).Distinct().ToList();
                List<string> aliveIds = group.Key switch
                {
                    LogEntityType.Order     => (await _repository.GetAllAsNoTracking<Order>()
                                                            .Where(x => ids.Contains(x.Id))
                                                            .Select(x => new { x.Id, x.IsArchived })
                                                            .ToListAsync())
                                                            .Select(x =>
                                                                    {
                                                                        if (x.IsArchived)
                                                                            archivedSet.Add((group.Key, x.Id));

                                                                        return x.Id;
                                                                    })
                                                            .ToList(),
                    LogEntityType.Job       => (await _repository.GetAllAsNoTracking<Job>()
                                                      .Where(x => ids.Contains(x.Id))
                                                      .Select(x => new { x.Id, x.Order.IsArchived })
                                                      .ToListAsync())
                                                      .Select(x =>
                                                              {
                                                                  if (x.IsArchived)
                                                                      archivedSet.Add((group.Key, x.Id));

                                                                  return x.Id;
                                                              })
                                                      .ToList(),
                    LogEntityType.Worker    => await _repository.GetAllAsNoTracking<Worker>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    LogEntityType.Client    => await _repository.GetAllAsNoTracking<Client>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    LogEntityType.Vehicle   => await _repository.GetAllAsNoTracking<Car>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    LogEntityType.Make      => await _repository.GetAllAsNoTracking<CarMake>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    LogEntityType.Model     => await _repository.GetAllAsNoTracking<CarModel>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    LogEntityType.JobType   => await _repository.GetAllAsNoTracking<JobType>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    LogEntityType.Part      => await _repository.GetAllAsNoTracking<Part>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    LogEntityType.Folder    => await _repository.GetAllAsNoTracking<PartsFolder>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    _ => new List<string>()
                };
                foreach (var id in aliveIds)
                    existenceSet.Add((group.Key, id));
            }

            // 3. Render final results
            var result = new List<ActivityLogVM>();
            bool ExistsChecker(LogEntityType type, string id) => existenceSet.Contains((type, id));
            bool ArchivedChecker(LogEntityType type, string id)
            {
                var checkType = type switch
                {
                    LogEntityType.ArchivedOrder => LogEntityType.Order,
                    LogEntityType.ArchivedJob   => LogEntityType.Job,
                    _                           => type
                };
                return archivedSet.Contains((checkType, id));
            }

            foreach (var log in logs)
            {
                if (logDataMap.TryGetValue(log, out var data))
                {
                    if (Enum.TryParse<LogEntityType>(log.LogType, true, out var entityType))
                    {
                        // Dynamically generate the message markup
                        var rendered = ActivityLogRenderer.Render(entityType, data, ExistsChecker, ArchivedChecker);
                        result.Add(new ActivityLogVM
                        {
                            Id = log.Id,
                            Timestamp = log.Timestamp,
                            Message = rendered.Header,
                            Details = rendered.Details
                        });
                    }
                }
            }

            return (result, totalCount);
        }

    }
}
