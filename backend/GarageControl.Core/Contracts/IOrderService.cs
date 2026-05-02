using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Shared;

namespace GarageControl.Core.Contracts
{
        public interface IOrderService
    {
        Task<List<OrderListVM>> GetOrdersAsync(string workshopId, bool? isArchived = null);
        Task<object> CreateOrderAsync(string userId, string workshopId, CreateOrderVM model);
        Task<OrderDetailsVM?> GetOrderByIdAsync(string id, string workshopId);
        Task<OrderInvoiceVM?> GetOrderInvoiceByIdAsync(string id);
        Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderVM model);
        Task<MethodResponseVM> DeleteOrderAsync(string userId, string id, string workshopId);
        Task<string> GenerateInvoiceAsync(string orderId, string workshopId);
    }
}