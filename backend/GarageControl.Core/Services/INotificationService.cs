using GarageControl.Core.ViewModels.Notifications;

namespace GarageControl.Core.Services
{
    public interface INotificationService
    {
        Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId, string userId);
        Task DeleteOldNotificationsAsync();
    }
}
