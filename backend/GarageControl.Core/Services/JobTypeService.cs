using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class JobTypeService : IJobTypeService
    {
        private readonly IRepository _repo;
        private readonly ICarServiceService _carServiceService;

        public JobTypeService(IRepository repo, ICarServiceService carServiceService)
        {
            _repo = repo;
            _carServiceService = carServiceService;
        }

        public async Task<IEnumerable<JobTypeVM>> All(string userId)
        {
            var serviceId = await _carServiceService.GetServiceId(userId);
            if (serviceId == null) return new List<JobTypeVM>();

            return await _repo.GetAllAsNoTrackingAsync<JobType>()
                .Where(j => j.CarServiceId == serviceId)
                .Include(j => j.Workers)
                .ThenInclude(w => w.User)
                .Select(j => new JobTypeVM
                {
                    Id = j.Id,
                    Name = j.Name,
                    Description = j.Description,
                    Mechanics = j.Workers.Select(w => w.User.UserName!).ToList()
                })
                .ToListAsync();
        }

        public async Task Create(JobTypeVM model, string userId)
        {
            var serviceId = await _carServiceService.GetServiceId(userId);
            if (serviceId == null) throw new ArgumentException("User does not have a service");

            var jobType = new JobType
            {
                Name = model.Name,
                Description = model.Description,
                CarServiceId = serviceId
            };

            // Handle mechanics mapping if needed
            // For now, simple implementation assuming mechanics are managed separately or we try to find them
            // Implementation omitted for string->Worker mapping complexity without explicit IDs

            await _repo.AddAsync(jobType);
            await _repo.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            await _repo.DeleteAsync<JobType>(id);
            await _repo.SaveChangesAsync();
        }

        public async Task<JobTypeVM?> Details(string id)
        {
            return await _repo.GetAllAsNoTrackingAsync<JobType>()
                .Where(j => j.Id == id)
                .Include(j => j.Workers)
                .ThenInclude(w => w.User)
                .Select(j => new JobTypeVM
                {
                    Id = j.Id,
                    Name = j.Name,
                    Description = j.Description,
                    Mechanics = j.Workers.Select(w => w.User.UserName!).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task Edit(JobTypeVM model)
        {
            var jobType = await _repo.GetByIdAsync<JobType>(model.Id);
            if (jobType != null)
            {
                jobType.Name = model.Name;
                jobType.Description = model.Description;
                
                // Note: Update mechanics relation here if feasible
                
                await _repo.SaveChangesAsync();
            }
        }
    }
}
