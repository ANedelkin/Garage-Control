using GarageControl.Core.ViewModels;

namespace GarageControl.Core.Contracts
{
    public interface IDashboardService
    {
        Task<DashboardVM> GetDashboardDataAsync(string workshopId);
    }
}