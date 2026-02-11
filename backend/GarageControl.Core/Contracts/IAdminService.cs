using GarageControl.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GarageControl.Core.Contracts
{
    public interface IAdminService
    {
        Task<List<UserAdminVM>> GetUsersAsync();
        Task<MethodResponseVM> ToggleUserBlockAsync(string userId);
        Task<List<WorkshopAdminVM>> GetWorkshopsAsync();
        Task<MethodResponseVM> ToggleWorkshopBlockAsync(string workshopId);
        Task<DashboardStatsVM> GetDashboardStatsAsync();
    }
}

