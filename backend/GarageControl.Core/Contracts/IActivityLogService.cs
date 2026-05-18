using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.Enums;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Contracts
{
    public interface IActivityLogService
    {

        // Task LogActionAsync(string userId, string workshopId, string actionMarkup);

        Task LogActionAsync(string userId, string workshopId, LogEntityType logType, ActivityLogData logData);

        Task<(IEnumerable<ActivityLogVM> Logs, int TotalCount)> GetLogsAsync(string workshopId, int page = 0, DateTime? startDate = null, DateTime? endDate = null, string? search = null);
    }
}
