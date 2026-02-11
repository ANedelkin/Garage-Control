using GarageControl.Core.ViewModels;

namespace GarageControl.Core.Contracts
{
    public interface INotificationService
    {
        Task<List<NotificationVM>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId, string userId);
        Task DeleteOldNotificationsAsync();
        Task SendStockNotificationAsync(string workshopId, string partId, string partName, double currentBalance, double minQuantity);
    }
}
