using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
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

        public async Task LogActionAsync(
            string userId, 
            string workshopId, 
            string action, 
            string? targetId, 
            string? targetName, 
            string? targetType)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            string actorName = user.Email ?? user.UserName ?? "Unknown";
            string actorType = "Worker";
            string? actorTargetId = null;

            var worker = await _context.Workers.AsNoTracking().FirstOrDefaultAsync(w => w.UserId == userId);
            if (worker != null)
            {
                actorName = worker.Name;
                actorTargetId = worker.Id;
            }

            var workshop = await _context.Workshops.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workshopId);
            if (workshop != null && workshop.BossId == userId)
            {
                actorType = "Owner";
            }

            var log = new ActivityLog
            {
                ActorId = userId,
                ActorTargetId = actorTargetId,
                ActorName = actorName,
                ActorType = actorType,
                Action = action,
                TargetId = targetId,
                TargetName = targetName,
                TargetType = targetType,
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
