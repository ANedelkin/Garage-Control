using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Models;
using System.Threading.Tasks;

namespace GarageControl.Core.Services
{
    public class OrderActivityLogger
    {
        private readonly IActivityLogService _activityLogService;

        public OrderActivityLogger(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public async Task LogOrderCreatedAsync(string userId, string workshopId, Order order)
        {
            string carName = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})";
            string orderLink = $"<a href='/orders' class='log-link target-link'>order for {carName}</a>";
            string message = $"created {orderLink}";

            await _activityLogService.LogActionAsync(userId, workshopId, message);
        }

        public async Task LogOrderUpdatedAsync(string userId, string workshopId, Order order)
        {
            string carName = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})";
            string orderLink = $"<a href='/orders' class='log-link target-link'>order for {carName}</a>";
            string message = $"updated {orderLink}";

            await _activityLogService.LogActionAsync(userId, workshopId, message);
        }
    }
}
