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
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"created part <a href='/parts?partId={partId}' class='log-link target-link'>{partName}</a>");
        }

        public async Task LogPartDeletedAsync(string userId, string workshopId, string partName)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"deleted part <b>{partName}</b>");
        }

        public async Task LogPartUpdatedAsync(string userId, string workshopId, string partId, string partName, List<ActivityPropertyChange> changes)
        {
            if (changes == null || !changes.Any()) return;

            var formattedChanges = changes.Select(c => 
            {
                string oldDisp = string.IsNullOrEmpty(c.OldValue) ? "[empty]" : c.OldValue;
                string newDisp = string.IsNullOrEmpty(c.NewValue) ? "[empty]" : c.NewValue;
                
                if (oldDisp.Length > 100 || newDisp.Length > 100)
                    return c.FieldName;
                    
                return $"{c.FieldName} from <b>{oldDisp}</b> to <b>{newDisp}</b>";
            }).ToList();

            string partLink = $"<a href='/parts?partId={partId}' class='log-link target-link'>{partName}</a>";
            string actionHtml = formattedChanges.Count == 1 && formattedChanges[0].Contains("from")
                ? $"changed {formattedChanges[0]} of part {partLink}"
                : formattedChanges.All(c => !c.Contains("from"))
                    ? $"updated details of part {partLink}"
                    : $"updated part {partLink}: {string.Join(", ", formattedChanges)}";

            await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
        }

        public async Task LogFolderCreatedAsync(string userId, string workshopId, string folderName)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"created group of parts <b>{folderName}</b>");
        }

        public async Task LogFolderDeletedAsync(string userId, string workshopId, string folderName)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"deleted group of parts <b>{folderName}</b>");
        }

        public async Task LogFolderRenamedAsync(string userId, string workshopId, string oldName, string newName)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"renamed group of parts <b>{oldName}</b> to <b>{newName}</b>");
        }

        public async Task LogPartMovedAsync(string userId, string workshopId, string partId, string partName, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"moved part <a href='/parts?partId={partId}' class='log-link target-link'>{partName}</a> from <b>{oldParent}</b> to <b>{newParent}</b>");
        }

        public async Task LogFolderMovedAsync(string userId, string workshopId, string folderName, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"moved group of parts <b>{folderName}</b> from <b>{oldParent}</b> to <b>{newParent}</b>");
        }
    }
}
