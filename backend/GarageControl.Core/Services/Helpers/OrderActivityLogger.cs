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

        public async Task LogOrderCreatedAsync(string userId, string workshopId, string carInfo)
        {
            string message = $"created {FormatOrderLink(carInfo)}";
            await _activityLogService.LogActionAsync(userId, workshopId, message);
        }

        public async Task LogOrderUpdatedAsync(string userId, string workshopId, string carInfo, List<ActivityPropertyChange> changes)
        {
            string actionHtml = $"updated {FormatOrderLink(carInfo)}";
            
            if (changes != null && changes.Any())
            {
                var formattedChanges = changes.Select(c => $"{c.FieldName} from <b>{c.OldValue}</b> to <b>{c.NewValue}</b>");
                actionHtml += $": {string.Join(", ", formattedChanges)}";
            }

            await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
        }

        private string FormatOrderLink(string carInfo) 
            => $"<a href='/orders' class='log-link target-link'>order for {carInfo}</a>";
    }
}
