using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface ICarServiceService
    {
        public Task<ServiceVM> GetServiceDetails(int serviceId);
        public Task<ServiceVM?> GetServiceDetailsByUser(string userId);
        public Task CreateService(string userId, ServiceVM model);
        public Task UpdateServiceDetails(string serviceId, ServiceVM model);
    }
}