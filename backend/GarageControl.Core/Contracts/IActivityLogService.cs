using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.Enums;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Contracts
{
    public interface IActivityLogService
    {
        /// <summary>Legacy overload — stores a hand-crafted markup string (kept for compatibility).</summary>
        Task LogActionAsync(string userId, string workshopId, string actionMarkup);

        /// <summary>
        /// Structured overload — stores <paramref name="logData"/> as JSON and renders
        /// Message automatically via <see cref="GarageControl.Core.Services.Helpers.ActivityLogRenderer"/>.
        /// </summary>
        Task LogActionAsync(string userId, string workshopId, LogEntityType logType, ActivityLogData logData);

        Task<(IEnumerable<ActivityLogVM> Logs, int TotalCount)> GetLogsAsync(string workshopId, int skip = 0, int take = 10, DateTime? startDate = null, DateTime? endDate = null, string? search = null);
    }
}
