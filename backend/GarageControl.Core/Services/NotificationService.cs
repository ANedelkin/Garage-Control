using GarageControl.Core.ViewModels.Notifications;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Core.Contracts;
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

        public async Task SendStockNotificationAsync(string workshopId, string partId, string partName, int currentBalance, int minQuantity)
        {
            // Find all users with access to "Parts Stockpile" for this workshop
            var usersToNotify = await _context.Workers
                .Where(w => w.WorkshopId == workshopId && w.Accesses.Any(a => a.Name == "Parts Stockpile"))
                .Select(w => w.UserId)
                .ToListAsync();

            // Also notify the owner
            var ownerId = await _context.Workshops
                .Where(w => w.Id == workshopId)
                .Select(w => w.BossId)
                .FirstOrDefaultAsync();

            if (ownerId != null && !usersToNotify.Contains(ownerId))
            {
                usersToNotify.Add(ownerId);
            }

            foreach (var userId in usersToNotify)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = $"Part '{partName}' is low on stock (Available: {currentBalance}, Minimum: {minQuantity}).",
                    Link = $"/parts?partId={partId}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }
    }
}
