using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Models;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GarageControl.Core.ViewModels.Parts;

namespace GarageControl.Core.Services.Helpers
{
    public class PartActivityLogger
    {
        private readonly IActivityLogService _activityLogService;

        public PartActivityLogger(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public async Task LogPartCreatedAsync(string userId, string workshopId, Part part)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"created part <a href='/parts?partId={part.Id}' class='log-link target-link'>{part.Name}</a>");
        }

        public async Task LogPartDeletedAsync(string userId, string workshopId, string partName)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"deleted part <b>{partName}</b>");
        }

        public async Task LogPartUpdatedAsync(string userId, string workshopId, Part part, List<string> changes)
        {
            if (!changes.Any()) return;

            string partLink = $"<a href='/parts?partId={part.Id}' class='log-link target-link'>{part.Name}</a>";
            string actionHtml = changes.Count == 1 && changes[0].Contains("from")
                ? $"changed {changes[0]} of part {partLink}"
                : changes.All(c => !c.Contains("from"))
                    ? $"updated details of part {partLink}"
                    : $"updated part {partLink}: {string.Join(", ", changes)}";

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

        public async Task LogPartMovedAsync(string userId, string workshopId, Part part, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"moved part <a href='/parts?partId={part.Id}' class='log-link target-link'>{part.Name}</a> from <b>{oldParent}</b> to <b>{newParent}</b>");
        }

        public async Task LogFolderMovedAsync(string userId, string workshopId, string folderName, string oldParent, string newParent)
        {
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"moved group of parts <b>{folderName}</b> from <b>{oldParent}</b> to <b>{newParent}</b>");
        }

        public List<string> TrackChanges(Part part, UpdatePartViewModel model)
        {
            var changes = new List<string>();
            string FormatPrice(decimal p) => p.ToString("0.00", CultureInfo.InvariantCulture);
            bool NumbersEqual(decimal? n1, decimal? n2) => (n1 ?? 0) == (n2 ?? 0);

            void Track(string fieldName, object? oldValue, object? newValue)
            {
                if (oldValue is decimal oldNum && newValue is decimal newNum)
                {
                    if (!NumbersEqual(oldNum, newNum))
                        changes.Add($"{fieldName} from <b>{FormatPrice(oldNum)}</b> to <b>{FormatPrice(newNum)}</b>");
                    return;
                }

                string oldStr = oldValue?.ToString() ?? "";
                string newStr = newValue?.ToString() ?? "";
                if (oldStr != newStr)
                {
                    string oldDisp = string.IsNullOrEmpty(oldStr) ? "[empty]" : oldStr;
                    string newDisp = string.IsNullOrEmpty(newStr) ? "[empty]" : newStr;

                    if (oldDisp.Length > 100 || newDisp.Length > 100)
                        changes.Add(fieldName);
                    else
                        changes.Add($"{fieldName} from <b>{oldDisp}</b> to <b>{newDisp}</b>");
                }
            }

            Track("name", part.Name, model.Name);
            Track("part number", part.PartNumber, model.PartNumber);
            Track("price", part.Price, model.Price);
            Track("quantity", part.Quantity, model.Quantity);
            Track("minimum quantity", part.MinimumQuantity, model.MinimumQuantity);

            return changes;
        }
    }
}
