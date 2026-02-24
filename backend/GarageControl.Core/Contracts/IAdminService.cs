using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Dashboard;
using GarageControl.Core.ViewModels.Shared;
using GarageControl.Core.ViewModels.Workshop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GarageControl.Core.Contracts
{
    public interface IAdminService
    {
        Task<List<UserAdminVM>> GetUsersAsync();
        Task<MethodResponseVM> ToggleUserBlockAsync(string userId, string? reason = null);
        Task<List<WorkshopAdminVM>> GetWorkshopsAsync();
        Task<MethodResponseVM> ToggleWorkshopBlockAsync(string workshopId, string? reason = null);
        Task<DashboardStatsVM> GetDashboardStatsAsync();
    }
}

