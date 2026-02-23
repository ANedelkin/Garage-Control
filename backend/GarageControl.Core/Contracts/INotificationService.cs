using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Shared;

namespace GarageControl.Core.Contracts
{
    public interface INotificationService
    {
        Task<List<NotificationVM>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId, string userId);
        Task DeleteOldNotificationsAsync();
        Task SendStockNotificationAsync(string workshopId, string partId, string partName, double currentBalance, double minQuantity);
        Task RemoveStockNotificationAsync(string workshopId, string partId);
    }
}
