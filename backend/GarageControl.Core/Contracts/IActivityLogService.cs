using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Contracts
{
    public interface IActivityLogService
    {
        Task LogActionAsync(string userId, string workshopId, string actionHtml);
        Task<IEnumerable<ActivityLog>> GetLogsAsync(string workshopId, int count = 100);
    }
}
