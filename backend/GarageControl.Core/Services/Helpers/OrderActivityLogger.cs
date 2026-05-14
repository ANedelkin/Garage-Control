using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GarageControl.Core.Models;
using GarageControl.Core.Enums;

namespace GarageControl.Core.Services
{
    public class OrderActivityLogger
    {
        private readonly IActivityLogService _activityLogService;

        public OrderActivityLogger(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public async Task LogOrderCreatedAsync(string userId, string workshopId, string orderId, string carInfo, List<ActivityPropertyChange>? changes = null)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Order,
                new ActivityLogData(LogAction.Created, orderId, carInfo, Changes: changes));
        }

        public async Task LogOrderUpdatedAsync(string userId, string workshopId, string orderId, string carInfo, List<ActivityPropertyChange> changes)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Order,
                new ActivityLogData(LogAction.Updated, orderId, carInfo, Changes: changes));
        }

        public async Task LogOrderArchivedAsync(string userId, string workshopId, string orderId, string carInfo, List<ActivityPropertyChange> changes)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Order,
                new ActivityLogData(LogAction.Archived, orderId, carInfo, Changes: changes));
        }

        public async Task LogOrderDeletedAsync(string userId, string workshopId, string carInfo)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Order,
                new ActivityLogData(LogAction.Deleted, null, carInfo));
        }
    }
}
