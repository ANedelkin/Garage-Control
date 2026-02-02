using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public interface IActivityLogService
    {
        Task LogActionAsync(string userId, string workshopId, string action, string? targetId, string? targetName, string? targetType);
        Task<IEnumerable<ActivityLog>> GetLogsAsync(string workshopId, int count = 100);
    }
}
