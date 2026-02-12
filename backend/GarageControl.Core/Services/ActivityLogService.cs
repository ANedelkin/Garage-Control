using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Core.Contracts;
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

        public async Task LogActionAsync(string userId, string workshopId, string actionHtml)
        {
            var query = from u in _repository.GetAllAsNoTracking<User>()
                        where u.Id == userId
                        let worker = _repository.GetAllAsNoTracking<Worker>()
                                        .Where(w => w.UserId == u.Id)
                                        .Select(w => new { w.Id, w.Name })
                                        .FirstOrDefault()
                        let workshop = _repository.GetAllAsNoTracking<Workshop>()
                                        .Where(ws => ws.Id == workshopId)
                                        .Select(ws => new { ws.BossId })
                                        .FirstOrDefault()
                        select new
                        {
                            UserName = worker.Name,
                            Worker = worker,
                            Workshop = workshop
                        };

            var result = await query.FirstOrDefaultAsync();
            if (result == null) return;

            string actorDisplayName = result.Workshop?.BossId == userId ? "Owner" : 
                                                                          result.Worker?.Name ?? 
                                                                          "Unknown User";

            string? actorLink = result.Worker != null ? $"/workers/{result.Worker.Id}" : null;

            string actorHtml = actorLink != null
                ? $"<a href='{actorLink}' class='log-link actor-link'>{actorDisplayName}</a>"
                : $"<span class='actor-name'>{actorDisplayName}</span>";

            await _repository.AddAsync(new ActivityLog
            {
                MessageHtml = $"{actorHtml} {actionHtml}",
                WorkshopId = workshopId,
            });

            await _repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsAsync(string workshopId, int count = 100)
        {
            return await _repository.GetAllAsNoTracking<ActivityLog>()
                .Where(l => l.WorkshopId == workshopId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .Select(l => new ActivityLog
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    WorkshopId = l.WorkshopId,
                    MessageHtml = l.MessageHtml
                })
                .ToListAsync();
        }
    }
}
