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
                MessageHtml = messageMarkup,
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

            var result = new List<ActivityLogVM>();

            foreach (var log in logs)
            {
                if (string.IsNullOrEmpty(log.LogType) || string.IsNullOrEmpty(log.LogData))
                {
                    result.Add(new ActivityLogVM
                    {
                        Id = log.Id,
                        Timestamp = log.Timestamp,
                        Message = log.MessageHtml
                    });
                    continue;
                }

                try
                {
                    var data = JsonSerializer.Deserialize<ActivityLogData>(log.LogData!, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    if (data != null)
                    {
                        var rendered = ActivityLogRenderer.Render(log.LogType!, data);
                        result.Add(new ActivityLogVM
                        {
                            Id = log.Id,
                            Timestamp = log.Timestamp,
                            Message = rendered.Header,
                            Details = rendered.Details
                        });
                    }
                }
                catch 
                { 
                    // Fallback to static message if parsing fails
                    result.Add(new ActivityLogVM
                    {
                        Id = log.Id,
                        Timestamp = log.Timestamp,
                        Message = log.MessageHtml
                    });
                }
            }

            return result;
        }


    }
}
