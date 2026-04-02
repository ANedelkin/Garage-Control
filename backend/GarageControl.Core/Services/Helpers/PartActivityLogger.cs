using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Models;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GarageControl.Core.Models;

namespace GarageControl.Core.Services.Helpers
{
    public class PartActivityLogger
    {
        private readonly IActivityLogService _activityLogService;

        public PartActivityLogger(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public async Task LogPartCreatedAsync(string userId, string workshopId, string partId, string partName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Part",
                new ActivityLogData("created", partId, partName));
        }

        public async Task LogPartDeletedAsync(string userId, string workshopId, string partName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Part",
                new ActivityLogData("deleted", null, partName));
        }

        public async Task LogPartUpdatedAsync(string userId, string workshopId, string partId, string partName, List<ActivityPropertyChange> changes)
        {
            if (changes == null || !changes.Any()) return;

            await _activityLogService.LogActionAsync(userId, workshopId, "Part",
                new ActivityLogData("updated", partId, partName, Changes: changes));
        }

        public async Task LogFolderCreatedAsync(string userId, string workshopId, string folderName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Folder",
                new ActivityLogData("created", null, folderName));
        }

        public async Task LogFolderDeletedAsync(string userId, string workshopId, string folderName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Folder",
                new ActivityLogData("deleted", null, folderName));
        }

        public async Task LogFolderRenamedAsync(string userId, string workshopId, string oldName, string newName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Folder",
                new ActivityLogData("renamed", null, oldName, SecondaryEntityName: newName));
        }

        public async Task LogPartMovedAsync(string userId, string workshopId, string partId, string partName, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Part",
                new ActivityLogData("moved", partId, partName, SecondaryEntityId: newParent, SecondaryEntityName: oldParent));
        }

        public async Task LogFolderMovedAsync(string userId, string workshopId, string folderName, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, "Folder",
                new ActivityLogData("moved", null, folderName, SecondaryEntityId: newParent, SecondaryEntityName: oldParent));
        }
    }
}
