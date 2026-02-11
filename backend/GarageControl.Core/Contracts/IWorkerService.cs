using GarageControl.Core.ViewModels;

namespace GarageControl.Core.Contracts
{
    public interface IWorkerService
    {
        Task<IEnumerable<WorkerVM>> All(string userId);
        Task<WorkerVM?> Details(string id);
        Task Create(WorkerVM model, string userId);
        Task Edit(string id, WorkerVM model, string userId);
        Task Delete(string id, string userId);
        Task<IEnumerable<AccessVM>> AllAccesses();
    }
}
