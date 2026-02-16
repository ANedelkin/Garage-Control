using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Clients;

namespace GarageControl.Core.Contracts
{
    public interface IClientService
    {
        Task<IEnumerable<ClientVM>> All(string userId);
        Task Create(ClientVM model, string userId);
        Task Edit(string id, ClientVM model, string userId);
        Task Delete(string id, string userId);
        Task<ClientVM?> Details(string id);
    }
}
