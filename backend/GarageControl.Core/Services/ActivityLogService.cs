using System.Text.Json;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.Services.Helpers;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IRepository _repository;

        public ActivityLogService(IRepository repository)
        {
            _repository = repository;
        }

        // ── Shared actor resolution ────────────────────────────────────────────

        private async Task<(string? actorId, string actorDisplayName)> ResolveActorInfoAsync(
            string userId, string workshopId)
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
                WorkshopId  = workshopId,
                LogType     = logType,
                LogData     = logData,
            });

            await _repository.SaveChangesAsync();
        }

        // ── IActivityLogService ────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task LogActionAsync(string userId, string workshopId, string actionMarkup)
        {
            var (actorId, actorName) = await ResolveActorInfoAsync(userId, workshopId);
            
            // Use markup for actor info
            string actorMarkup = actorId != null
                ? $"[{actorName ?? "[Unknown]"}](/workers/{actorId}?highlight=true)"
                : $"**{actorName ?? "[Unknown]"}**";

            await SaveLogAsync(workshopId, $"{actorMarkup} {actionMarkup}");
        }

        /// <inheritdoc/>
        public async Task LogActionAsync(
            string userId,
            string workshopId,
            string logType,
            ActivityLogData logData)
        {
            var (actorId, actorName) = await ResolveActorInfoAsync(userId, workshopId);
            
            // Enrich log data with actor info for dynamic rendering later
            logData = logData with { ActorId = actorId, ActorName = actorName };

            // Use markup for actor info
            string actorMarkup = actorId != null
                ? $"[{actorName ?? "[Unknown]"}](/workers/{actorId}?highlight=true)"
                : $"**{actorName}**";

            string messageMarkup = ActivityLogRenderer.BuildMessageMarkup(logType, logData);
            if (string.IsNullOrEmpty(messageMarkup)) return;

            string serialised = JsonSerializer.Serialize(logData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            await SaveLogAsync(workshopId, messageMarkup, logType, serialised);
        }

        public async Task<IEnumerable<ActivityLogVM>> GetLogsAsync(string workshopId, int count = 100)
        {
            var logs = await _repository.GetAllAsNoTracking<ActivityLog>()
                .Where(l => l.WorkshopId == workshopId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();

            // 1. Deserialize and collect all referenced entities to pre-fetch existence
            var logDataMap = new Dictionary<ActivityLog, ActivityLogData>();
            var referenced = new HashSet<(string Type, string Id)>();

            foreach (var log in logs)
            {
                if (!string.IsNullOrEmpty(log.LogType) && !string.IsNullOrEmpty(log.LogData))
                {
                    try
                    {
                        var data = JsonSerializer.Deserialize<ActivityLogData>(log.LogData!, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        if (data != null)
                        {
                            logDataMap[log] = data;
                            foreach (var r in ActivityLogRenderer.GetReferencedEntities(log.LogType, data))
                            {
                                referenced.Add(r);
                            }
                        }
                    }
                    catch { /* Skip malformed logs */ }
                }
            }

            // 2. Check existence in bulk to avoid N+1
            var existenceSet = new HashSet<(string Type, string Id)>();
            var byType = referenced.GroupBy(r => r.Type);

            foreach (var group in byType)
            {
                var ids = group.Select(r => r.Id).Distinct().ToList();
                List<string> aliveIds = group.Key switch
                {
                    "Worker"   => await _repository.GetAllAsNoTracking<Worker>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Client"   => await _repository.GetAllAsNoTracking<Client>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Vehicle"  => await _repository.GetAllAsNoTracking<Car>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Make"     => await _repository.GetAllAsNoTracking<CarMake>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Model"    => await _repository.GetAllAsNoTracking<CarModel>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "JobType"  => await _repository.GetAllAsNoTracking<JobType>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Order"    => await _repository.GetAllAsNoTracking<Order>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Job"      => await _repository.GetAllAsNoTracking<Job>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Part"     => await _repository.GetAllAsNoTracking<Part>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    "Folder"   => await _repository.GetAllAsNoTracking<PartsFolder>().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(),
                    _          => new List<string>()
                };

                foreach (var id in aliveIds)
                {
                    existenceSet.Add((group.Key, id));
                }
            }

            // 3. Render final results
            var result = new List<ActivityLogVM>();
            bool ExistsChecker(string type, string id) => existenceSet.Contains((type, id));

            foreach (var log in logs)
            {
                if (logDataMap.TryGetValue(log, out var data))
                {
                    var rendered = ActivityLogRenderer.Render(log.LogType!, data, ExistsChecker);
                    result.Add(new ActivityLogVM
                    {
                        Id = log.Id,
                        Timestamp = log.Timestamp,
                        Message = rendered.Header,
                        Details = rendered.Details
                    });
                }
                else
                {
                    // Fallback to static message for legacy logs or if data is missing
                    result.Add(new ActivityLogVM
                    {
                        Id = log.Id,
                        Timestamp = log.Timestamp,
                        Message = log.MessageMarkup
                    });
                }
            }

            return result;
        }


    }
}
