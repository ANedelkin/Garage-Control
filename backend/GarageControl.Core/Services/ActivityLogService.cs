using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly GarageControlDbContext _context;

        public ActivityLogService(GarageControlDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string userId, string workshopId, string actionHtml)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            string actorDisplayName;
            string? actorLink = null;

            var worker = await _context.Workers.AsNoTracking().FirstOrDefaultAsync(w => w.UserId == userId);
            var workshop = await _context.Workshops.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workshopId);

            if (workshop != null && workshop.BossId == userId)
            {
                actorDisplayName = "Owner";
            }
            else if (worker != null)
            {
                actorDisplayName = worker.Name;
                actorLink = $"/workers/{worker.Id}";
            }
            else
            {
                actorDisplayName = user.UserName ?? "Unknown";
            }

            string actorHtml = actorLink != null 
                ? $"<a href='{actorLink}' class='log-link actor-link'>{actorDisplayName}</a>" 
                : $"<span class='actor-name'>{actorDisplayName}</span>";

            var log = new ActivityLog
            {
                MessageHtml = $"{actorHtml} {actionHtml}",
                WorkshopId = workshopId,
                Timestamp = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsAsync(string workshopId, int count = 100)
        {
            return await _context.ActivityLogs
                .Where(l => l.WorkshopId == workshopId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
