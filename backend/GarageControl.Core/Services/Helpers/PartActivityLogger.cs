using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Models;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GarageControl.Core.Models;
using GarageControl.Core.Enums;

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
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Part,
                new ActivityLogData(LogAction.Created, partId, partName));
        }

        public async Task LogPartDeletedAsync(string userId, string workshopId, string partName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Part,
                new ActivityLogData(LogAction.Deleted, null, partName));
        }

        public async Task LogPartUpdatedAsync(string userId, string workshopId, string partId, string partName, List<ActivityPropertyChange> changes)
        {
            if (changes == null || !changes.Any()) return;

            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Part,
                new ActivityLogData(LogAction.Updated, partId, partName, Changes: changes));
        }

        public async Task LogFolderCreatedAsync(string userId, string workshopId, string folderName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Folder,
                new ActivityLogData(LogAction.Created, null, folderName));
        }

        public async Task LogFolderDeletedAsync(string userId, string workshopId, string folderName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Folder,
                new ActivityLogData(LogAction.Deleted, null, folderName));
        }

        public async Task LogFolderRenamedAsync(string userId, string workshopId, string oldName, string newName)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Folder,
                new ActivityLogData(LogAction.Renamed, null, oldName, SecondaryEntityName: newName));
        }

        public async Task LogPartMovedAsync(string userId, string workshopId, string partId, string partName, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Part,
                new ActivityLogData(LogAction.Moved, partId, partName, SecondaryEntityId: newParent, SecondaryEntityName: oldParent));
        }

        public async Task LogFolderMovedAsync(string userId, string workshopId, string folderName, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(userId, workshopId, LogEntityType.Folder,
                new ActivityLogData(LogAction.Moved, null, folderName, SecondaryEntityId: newParent, SecondaryEntityName: oldParent));
        }
    }
}
