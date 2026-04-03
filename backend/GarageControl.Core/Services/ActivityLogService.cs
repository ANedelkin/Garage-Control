using System.Text.Json;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
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

            var workshop = await _repository.GetAllAsNoTracking<Workshop>()
                .Where(ws => ws.Id == workshopId)
                .Select(ws => new { ws.BossId })
                .FirstOrDefaultAsync();

            string actorDisplayName = workshop?.BossId == userId
                ? "Owner"
                : worker?.Name ?? "Unknown User";

            return (worker?.Id, actorDisplayName);
        }

        private async Task SaveLogAsync(
            string workshopId,
            string messageHtml,
            string? logType = null,
            string? logData = null)
        {
            await _repository.AddAsync(new ActivityLog
            {
                MessageHtml = messageHtml,
                WorkshopId  = workshopId,
                LogType     = logType,
                LogData     = logData,
            });

            await _repository.SaveChangesAsync();
        }

        // ── IActivityLogService ────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task LogActionAsync(string userId, string workshopId, string actionHtml)
        {
            var (actorId, actorName) = await ResolveActorInfoAsync(userId, workshopId);
            
            // For legacy/simple logs, we still build a static string but save actor info if available
            string actorHtml = actorId != null && actorName != "Owner"
                ? $"<a href='/workers/{actorId}?highlight=true' class='log-link actor-link'>{actorName}</a>"
                : $"<span class='actor-name'>{actorName}</span>";

            await SaveLogAsync(workshopId, $"{actorHtml} {actionHtml}");
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

            // We still build a "legacy" messageHtml for compatibility with current DB schema
            // but GetLogsAsync will now override it.
            string actorHtml = actorId != null && actorName != "Owner"
                ? $"<a href='/workers/{actorId}?highlight=true' class='log-link actor-link'>{actorName}</a>"
                : $"<span class='actor-name'>{actorName}</span>";

            string messageHtml = ActivityLogRenderer.BuildMessageHtml(logType, logData, null);
            if (string.IsNullOrEmpty(messageHtml)) return;

            string serialised = JsonSerializer.Serialize(logData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            await SaveLogAsync(workshopId, messageHtml, logType, serialised);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ActivityLog>> GetLogsAsync(string workshopId, int count = 100)
        {
            var logs = await _repository.GetAllAsNoTracking<ActivityLog>()
                .Where(l => l.WorkshopId == workshopId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();

            // Perform dynamic re-rendering for structured logs
            var structuredLogs = logs.Where(l => !string.IsNullOrEmpty(l.LogType) && !string.IsNullOrEmpty(l.LogData)).ToList();
            if (structuredLogs.Any())
            {
                var logDatas = new List<(ActivityLog log, ActivityLogData data)>();
                var allIds = new HashSet<string>();

                foreach (var log in structuredLogs)
                {
                    try 
                    {
                        var data = JsonSerializer.Deserialize<ActivityLogData>(log.LogData!, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        if (data != null)
                        {
                            logDatas.Add((log, data));
                            if (!string.IsNullOrEmpty(data.ActorId)) allIds.Add(data.ActorId);
                            if (!string.IsNullOrEmpty(data.EntityId)) allIds.Add(data.EntityId);
                            if (!string.IsNullOrEmpty(data.SecondaryEntityId)) allIds.Add(data.SecondaryEntityId);
                            
                            if (data.Changes != null)
                            {
                                foreach (var c in data.Changes.Where(ch => ch.FieldName == "mechanic" && ch.NewValue != null))
                                {
                                    var parts = c.NewValue!.Split('|');
                                    if (parts.Length == 2) allIds.Add(parts[1]);
                                }
                            }
                        }
                    }
                    catch { /* Skip malformed logs */ }
                }

                if (allIds.Any())
                {
                    var liveNames = await GetLiveNamesAsync(allIds);
                    foreach (var (log, data) in logDatas)
                    {
                        log.MessageHtml = ActivityLogRenderer.BuildMessageHtml(log.LogType!, data, liveNames);
                    }
                }
            }

            return logs;
        }

        private async Task<Dictionary<string, string>> GetLiveNamesAsync(HashSet<string> ids)
        {
            var result = new Dictionary<string, string>();
            if (ids == null || !ids.Any()) return result;

            // Fetch live names from all relevant tables
            // This is efficient because it uses primary key indices
            
            var workers = await _repository.GetAllAsNoTracking<Worker>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            foreach (var x in workers) result[x.Id] = x.Name;

            var clients = await _repository.GetAllAsNoTracking<Client>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            foreach (var x in clients) result[x.Id] = x.Name;

            var cars = await _repository.GetAllAsNoTracking<Car>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.RegistrationNumber }).ToListAsync();
            foreach (var x in cars) result[x.Id] = x.RegistrationNumber;

            var parts = await _repository.GetAllAsNoTracking<Part>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            foreach (var x in parts) result[x.Id] = x.Name;

            var folders = await _repository.GetAllAsNoTracking<PartsFolder>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            foreach (var x in folders) result[x.Id] = x.Name;

            var jobTypes = await _repository.GetAllAsNoTracking<JobType>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            foreach (var x in jobTypes) result[x.Id] = x.Name;

            var makes = await _repository.GetAllAsNoTracking<CarMake>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            foreach (var x in makes) result[x.Id] = x.Name;

            var models = await _repository.GetAllAsNoTracking<CarModel>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            foreach (var x in models) result[x.Id] = x.Name;

            // For complex entities like Orders/Jobs, we mostly check existence. 
            // If they exist, we often don't have a simple single "Name" field to fetch, 
            // but their presence in the dictionary marks them as "live" for the renderer.
            var orders = await _repository.GetAllAsNoTracking<Order>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id }).ToListAsync();
            foreach (var x in orders) if (!result.ContainsKey(x.Id)) result[x.Id] = null!; // null marks as "exists but use default name"

            var jobs = await _repository.GetAllAsNoTracking<Job>().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id }).ToListAsync();
            foreach (var x in jobs) if (!result.ContainsKey(x.Id)) result[x.Id] = null!;

            return result;
        }
    }
}
