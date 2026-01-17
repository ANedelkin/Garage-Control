using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IWorkshopService
    {
        public Task<WorkshopVM> GetWorkshopDetails(string workshopId);
        public Task<WorkshopVM?> GetWorkshopDetailsByUser(string userId);
        public Task CreateWorkshop(string userId, WorkshopVM model);
        public Task UpdateWorkshopDetails(string ownerId, WorkshopVM model);
        public Task<string?> GetWorkshopId(string userId);
        public Task<string?> GetWorkshopBossId(string userId);
    }
}
