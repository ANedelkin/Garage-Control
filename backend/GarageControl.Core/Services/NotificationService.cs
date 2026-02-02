using GarageControl.Core.ViewModels.Notifications;
using GarageControl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly GarageControlDbContext _context;

        public NotificationService(GarageControlDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    Link = n.Link,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(string notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOldNotificationsAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < thirtyDaysAgo)
                .ToListAsync();

            if (oldNotifications.Any())
            {
                _context.Notifications.RemoveRange(oldNotifications);
                await _context.SaveChangesAsync();
            }
        }
    }
}
