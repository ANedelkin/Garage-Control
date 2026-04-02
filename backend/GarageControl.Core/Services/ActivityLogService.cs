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

        private async Task<(string actorHtml, string workshopIdResolved)> ResolveActorAsync(
            string userId, string workshopId)
        {
            var userExists = await _repository.GetAllAsNoTracking<User>()
                .AnyAsync(u => u.Id == userId);

            if (!userExists) return (string.Empty, workshopId);

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

            string? actorLink = worker != null
                ? $"/workers/{worker.Id}?highlight=true"
                : null;

            string actorHtml = actorLink != null
                ? $"<a href='{actorLink}' class='log-link actor-link'>{actorDisplayName}</a>"
                : $"<span class='actor-name'>{actorDisplayName}</span>";

            return (actorHtml, workshopId);
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
            var (actorHtml, _) = await ResolveActorAsync(userId, workshopId);
            if (string.IsNullOrEmpty(actorHtml)) return;

            await SaveLogAsync(workshopId, $"{actorHtml} {actionHtml}");
        }

        /// <inheritdoc/>
        public async Task LogActionAsync(
            string userId,
            string workshopId,
            string logType,
            ActivityLogData logData)
        {
            var (actorHtml, _) = await ResolveActorAsync(userId, workshopId);
            if (string.IsNullOrEmpty(actorHtml)) return;

            // Instead of saving pre-rendered HTML in the database, store it in the JSON for backwards compatibility/fallback,
            // but we will generate the actual HTML dynamically on fetch to verify if IDs still exist!
            var logDataWithActor = logData with { ActorHtmlFallback = actorHtml };

            string serialised = JsonSerializer.Serialize(logDataWithActor, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            await SaveLogAsync(workshopId, "", logType, serialised);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ActivityLog>> GetLogsAsync(string workshopId, int count = 100)
        {
            var logs = await _repository.GetAllAsNoTracking<ActivityLog>()
                .Where(l => l.WorkshopId == workshopId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();

            var logDatas = new Dictionary<string, ActivityLogData>();
            var allIds = new HashSet<string>();

            // 1. Gather all IDs we might need to check for existence
            foreach (var log in logs)
            {
                if (!string.IsNullOrEmpty(log.LogData))
                {
                    try
                    {
                        var data = JsonSerializer.Deserialize<ActivityLogData>(log.LogData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        if (data != null)
                        {
                            logDatas[log.Id] = data;
                            if (data.EntityId != null) allIds.Add(data.EntityId);
                            if (data.SecondaryEntityId != null) allIds.Add(data.SecondaryEntityId);
                            if (data.Changes != null)
                            {
                                foreach (var c in data.Changes)
                                {
                                    if (c.EntityId != null) allIds.Add(c.EntityId);
                                }
                            }
                        }
                    }
                    catch { /* ignore invalid json */ }
                }
            }

            // 2. Query the DB for existing IDs
            var validIds = new HashSet<string>();
            var idList = allIds.ToList();

            if (idList.Any())
            {
                var workers = await _repository.GetAllAsNoTracking<Worker>().Where(w => idList.Contains(w.Id)).Select(w => w.Id).ToListAsync();
                var clients = await _repository.GetAllAsNoTracking<Client>().Where(c => idList.Contains(c.Id)).Select(c => c.Id).ToListAsync();
                var cars = await _repository.GetAllAsNoTracking<Car>().Where(c => idList.Contains(c.Id)).Select(c => c.Id).ToListAsync();
                var parts = await _repository.GetAllAsNoTracking<Part>().Where(p => idList.Contains(p.Id)).Select(p => p.Id).ToListAsync();
                var orders = await _repository.GetAllAsNoTracking<Order>().Where(o => idList.Contains(o.Id)).Select(o => o.Id).ToListAsync();
                var jobs = await _repository.GetAllAsNoTracking<Job>().Where(j => idList.Contains(j.Id)).Select(j => j.Id).ToListAsync();
                var makes = await _repository.GetAllAsNoTracking<CarMake>().Where(m => idList.Contains(m.Id)).Select(m => m.Id).ToListAsync();
                var models = await _repository.GetAllAsNoTracking<CarModel>().Where(m => idList.Contains(m.Id)).Select(m => m.Id).ToListAsync();
                var jobTypes = await _repository.GetAllAsNoTracking<JobType>().Where(j => idList.Contains(j.Id)).Select(j => j.Id).ToListAsync();
                var folders = await _repository.GetAllAsNoTracking<PartsFolder>().Where(f => idList.Contains(f.Id)).Select(f => f.Id).ToListAsync();
                
                validIds.UnionWith(workers);
                validIds.UnionWith(clients);
                validIds.UnionWith(cars);
                validIds.UnionWith(parts);
                validIds.UnionWith(orders);
                validIds.UnionWith(jobs);
                validIds.UnionWith(makes);
                validIds.UnionWith(models);
                validIds.UnionWith(jobTypes);
                validIds.UnionWith(folders);
            }

            // 3. Rebuild MessageHtml dynamically
            return logs.Select(l =>
            {
                string messageHtml = l.MessageHtml;

                if (logDatas.TryGetValue(l.Id, out var data))
                {
                    string? actorHtml = data.ActorHtmlFallback;

                    if (string.IsNullOrEmpty(actorHtml) && !string.IsNullOrEmpty(l.MessageHtml))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(l.MessageHtml, @"^<span class='actor-name'>.*?</span>");
                        if (match.Success)
                        {
                            actorHtml = match.Value;
                        }
                    }

                    if (!string.IsNullOrEmpty(actorHtml))
                    {
                        string renderedHtml = ActivityLogRenderer.BuildMessageHtml(actorHtml, l.LogType ?? "", data, id => validIds.Contains(id));
                        if (!string.IsNullOrEmpty(renderedHtml))
                        {
                            messageHtml = renderedHtml;
                        }
                    }
                }

                return new ActivityLog
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    WorkshopId = l.WorkshopId,
                    MessageHtml = messageHtml,
                    LogType = l.LogType,
                    LogData = l.LogData,
                };
            }).ToList();
        }
    }
}
