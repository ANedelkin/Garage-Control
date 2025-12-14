using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IClientService
    {
        Task<IEnumerable<ClientVM>> All(string userId);
        Task Create(ClientVM model, string userId);
        Task Edit(ClientVM model);
        Task Delete(string id);
        Task<ClientVM?> Details(string id);
    }
}
