using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Dashboard;

namespace GarageControl.Core.Contracts
{
    public interface IDashboardService
    {
        Task<DashboardVM> GetDashboardDataAsync(string workshopId);
    }
}