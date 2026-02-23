using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Shared;
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

        public async Task<List<NotificationVM>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationVM
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

        public async Task SendStockNotificationAsync(string workshopId, string partId, string partName, double currentBalance, double minQuantity)
        {
            // Find all users with access to "Parts Stock" for this workshop
            var usersToNotify = await _context.Workers
                .Where(w => w.WorkshopId == workshopId && w.Accesses.Any(a => a.Name == "Parts Stock"))
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

            var link = $"/parts?partId={partId}";
            var existingNotifications = await _context.Notifications
                .Where(n => usersToNotify.Contains(n.UserId) && n.Link == link)
                .ToListAsync();

            foreach (var userId in usersToNotify)
            {
                var existing = existingNotifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .FirstOrDefault();

                var message = $"Part '{partName}' is low on stock (Available: {currentBalance}, Minimum: {minQuantity}).";

                if (existing != null)
                {
                    existing.Message = message;
                    existing.CreatedAt = DateTime.UtcNow;
                    existing.IsRead = false;
                }
                else
                {
                    var notification = new Notification
                    {
                        UserId = userId,
                        Message = message,
                        Link = link,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveStockNotificationAsync(string workshopId, string partId)
        {
            var link = $"/parts?partId={partId}";
            
            var notificationsToDelete = await _context.Notifications
                .Where(n => n.Link == link && n.Message.Contains("low on stock"))
                .ToListAsync();

            if (notificationsToDelete.Any())
            {
                _context.Notifications.RemoveRange(notificationsToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
