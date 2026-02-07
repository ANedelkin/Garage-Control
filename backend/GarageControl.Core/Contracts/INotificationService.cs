using GarageControl.Core.ViewModels.Notifications;

namespace GarageControl.Core.Contracts
{
    public interface INotificationService
    {
        Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId, string userId);
        Task DeleteOldNotificationsAsync();
        Task SendStockNotificationAsync(string workshopId, string partId, string partName, int currentBalance, int minQuantity);
    }
}
