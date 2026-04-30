using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Contracts
{
    public interface IActivityLogService
    {
        /// <summary>Legacy overload — stores a hand-crafted HTML string (kept for compatibility).</summary>
        Task LogActionAsync(string userId, string workshopId, string actionHtml);

        /// <summary>
        /// Structured overload — stores <paramref name="logData"/> as JSON and renders
        /// <c>MessageHtml</c> automatically via <see cref="GarageControl.Core.Services.Helpers.ActivityLogRenderer"/>.
        /// </summary>
        Task LogActionAsync(string userId, string workshopId, string logType, ActivityLogData logData);

        Task<IEnumerable<ActivityLogVM>> GetLogsAsync(string workshopId, int count = 100);
    }
}
