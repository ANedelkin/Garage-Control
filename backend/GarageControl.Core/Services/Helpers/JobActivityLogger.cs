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
            var changes = partChanges?.Select(p => new ActivityPropertyChange(p, "", "")).ToList();

            await _activityLogService.LogActionAsync(userId, workshopId, "Job",
                new ActivityLogData("created", jobId, jobTypeName,
                    SecondaryEntityId: orderId, SecondaryEntityName: carInfo,
                    Changes: changes));
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
            var allChanges = new List<ActivityPropertyChange>();

            if (propertyChanges != null) allChanges.AddRange(propertyChanges);
            if (partChanges != null) allChanges.AddRange(partChanges.Select(p => new ActivityPropertyChange(p, "", "")));

            if (!allChanges.Any()) return;

            await _activityLogService.LogActionAsync(userId, workshopId, "Job",
                new ActivityLogData("updated", jobId, jobTypeName,
                    SecondaryEntityId: orderId, SecondaryEntityName: carInfo,
                    Changes: allChanges));
        }

        public async Task LogJobDeletedAsync(
            string userId,
            string workshopId,
            string orderId,
            string jobTypeName,
            string carInfo)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Job",
                new ActivityLogData("deleted", null, jobTypeName,
                    SecondaryEntityId: orderId, SecondaryEntityName: carInfo));
        }

        public string FormatPartAdded(string partId, string partName)
        {
            string link = partId != null ? $"<a href='/parts?partId={partId}' class='log-link target-link'>{partName}</a>" : $"<b>{partName}</b>";
            return $"added part '{link}'";
        }

        public string FormatPartRemoved(string partId, string partName)
        {
            string link = partId != null ? $"<a href='/parts?partId={partId}' class='log-link target-link'>{partName}</a>" : $"<b>{partName}</b>";
            return $"removed part '{link}'";
        }

        public string FormatPartQuantityChanged(string partId, string partName, string qtyType, string oldVal, string newVal)
        {
            string link = partId != null ? $"<a href='/parts?partId={partId}' class='log-link target-link'>{partName}</a>" : $"<b>{partName}</b>";
            return $"changed {qtyType} qty of '{link}' from <b>{oldVal}</b> to <b>{newVal}</b>";
        }
    }
}
