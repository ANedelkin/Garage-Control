using GarageControl.Core.Models;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;

namespace GarageControl.Core.Contracts
{
        public interface IOrderService
    {
        Task<List<OrderListViewModel>> GetOrdersAsync(string workshopId, bool? isDone = null);
        Task<object> CreateOrderAsync(string userId, string workshopId, CreateOrderViewModel model);
        Task<OrderDetailsViewModel?> GetOrderByIdAsync(string id, string workshopId);
        Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderViewModel model);
    }
}