using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleVM>> All(string userId);
        Task<IEnumerable<VehicleVM>> GetByClient(string clientId);
        Task Create(VehicleVM model, string userId);
        Task Edit(VehicleVM model, string userId);
        Task Delete(string id, string userId);
        Task<VehicleVM?> Details(string id);
    }
}
