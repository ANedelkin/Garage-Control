using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IClientService
    {
        Task<IEnumerable<ClientVM>> All(string userId);
        Task Create(ClientVM model, string userId);
        Task Edit(ClientVM model, string userId);
        Task Delete(string id, string userId);
        Task<ClientVM?> Details(string id);
    }
}
