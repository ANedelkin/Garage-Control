using GarageControl.Core.ViewModels;

namespace GarageControl.Core.Contracts
{
    public interface IJobTypeService
    {
        Task<IEnumerable<JobTypeVM>> All(string userId);
        Task<JobTypeVM?> Details(string id);
        Task Create(JobTypeVM model, string userId);
        Task Edit(string id, JobTypeVM model, string userId);
        Task Delete(string id, string userId);
    }
}
