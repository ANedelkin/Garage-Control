using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GarageControl.Core.Models;

namespace GarageControl.Core.Services
{
    public class OrderActivityLogger
    {
        private readonly IActivityLogService _activityLogService;

        public OrderActivityLogger(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public async Task LogOrderCreatedAsync(string userId, string workshopId, string orderId, string carInfo)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Order",
                new ActivityLogData("created", orderId, carInfo));
        }

        public async Task LogOrderUpdatedAsync(string userId, string workshopId, string orderId, string carInfo, List<ActivityPropertyChange> changes)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Order",
                new ActivityLogData("updated", orderId, carInfo, Changes: changes));
        }
    }
}
