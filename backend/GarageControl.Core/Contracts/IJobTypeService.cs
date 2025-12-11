using GarageControl.Core.Models;

namespace GarageControl.Core.Contracts
{
    public interface IJobTypeService
    {
        Task<IEnumerable<JobTypeVM>> All(string userId);
        Task<JobTypeVM?> Details(string id);
        Task Create(JobTypeVM model, string userId);
        Task Edit(JobTypeVM model);
        Task Delete(string id);
    }
}
