using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
using GarageControl.Core.ViewModels.Workshop;

namespace GarageControl.Core.Contracts
{
    public interface IWorkshopService
    {
        public Task<WorkshopVM> GetWorkshopDetails(string workshopId);
        public Task<WorkshopVM?> GetWorkshopDetailsByUser(string userId);
        public Task<LoginResponseVM> CreateWorkshop(string userId, WorkshopVM model);
        public Task UpdateWorkshopDetails(string ownerId, WorkshopVM model);
        public Task<string?> GetWorkshopId(string userId);
        public Task<string?> GetWorkshopBossId(string userId);
    }
}
