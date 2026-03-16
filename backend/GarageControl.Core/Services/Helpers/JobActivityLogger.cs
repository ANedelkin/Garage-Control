using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GarageControl.Core.Services.Jobs
{
    public class JobActivityLogger
    {
        private readonly IActivityLogService _activityLogService;

        public JobActivityLogger(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public async Task LogJobCreatedAsync(
            string userId,
            string workshopId,
            string orderId,
            string jobId,
            string jobTypeName,
            string carInfo,
            List<string> partChanges)
        {
            string actionHtml = $"created job {FormatJobLink(orderId, jobId, jobTypeName)} for {FormatOrderLink(orderId, carInfo)}";
            if (partChanges != null && partChanges.Any())
                actionHtml += $": {string.Join(", ", partChanges)}";

            await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
        }

        public async Task LogJobUpdatedAsync(
            string userId,
            string workshopId,
            string orderId,
            string jobId,
            string jobTypeName,
            string carInfo,
            List<ActivityPropertyChange> propertyChanges,
            List<string> partChanges)
        {
            var allChanges = new List<string>();

            if (propertyChanges != null)
            {
                allChanges.AddRange(propertyChanges.Select(c => $"{c.FieldName} from <b>{c.OldValue}</b> to <b>{c.NewValue}</b>"));
            }

            if (partChanges != null)
            {
                allChanges.AddRange(partChanges);
            }

            if (!allChanges.Any()) return;

            string actionHtml = $"updated job {FormatJobLink(orderId, jobId, jobTypeName)} for {FormatOrderLink(orderId, carInfo)}: {string.Join(", ", allChanges)}";
            await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
        }
        public async Task LogJobDeletedAsync(
            string userId,
            string workshopId,
            string orderId,
            string jobTypeName,
            string carInfo)
        {
            string actionHtml = $"deleted job '{jobTypeName}' for {FormatOrderLink(orderId, carInfo)}";
            await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
        }

        public string FormatPartAdded(string partName) => $"added part '<b>{partName}</b>'";
        public string FormatPartRemoved(string partName) => $"removed part '<b>{partName}</b>'";
        public string FormatPartQuantityChanged(string partName, string qtyType, string oldVal, string newVal) 
            => $"changed {qtyType} qty of '<b>{partName}</b>' from <b>{oldVal}</b> to <b>{newVal}</b>";

        private string FormatOrderLink(string orderId, string carInfo) 
            => $"<a href='/orders/{orderId}?highlight=true' class='log-link target-link'>order for {carInfo}</a>";
            
        private string FormatJobLink(string orderId, string jobId, string jobTypeName)
            => $"<a href='/orders/{orderId}?highlightJob={jobId}' class='log-link target-link'>'{jobTypeName}'</a>";
    }
}
