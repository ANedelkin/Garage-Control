using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
using GarageControl.Core.ViewModels.Shared;
using GarageControl.Core.ViewModels.Workers;

namespace GarageControl.Core.Contracts
{
    public interface IWorkerService
    {
        Task<IEnumerable<WorkerVM>> All(string userId);
        Task<WorkerVM?> Details(string id);
        Task<MethodResponseVM> Create(WorkerVM model, string userId);
        Task<MethodResponseVM> Edit(string id, WorkerVM model, string userId);
        Task Delete(string id, string userId);
        Task<IEnumerable<AccessVM>> AllAccesses();
    }
}
