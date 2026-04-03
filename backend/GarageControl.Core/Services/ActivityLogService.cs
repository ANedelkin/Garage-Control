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

            string messageHtml = ActivityLogRenderer.BuildMessageHtml(actorHtml, logType, logData);
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
            return await _repository.GetAllAsNoTracking<ActivityLog>()
                .Where(l => l.WorkshopId == workshopId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .Select(l => new ActivityLog
                {
                    Id          = l.Id,
                    Timestamp   = l.Timestamp,
                    WorkshopId  = l.WorkshopId,
                    MessageHtml = l.MessageHtml,
                    LogType     = l.LogType,
                    LogData     = l.LogData,
                })
                .ToListAsync();
        }
    }
}
