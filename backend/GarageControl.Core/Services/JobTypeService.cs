using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class JobTypeService : IJobTypeService
    {
        private readonly IRepository _repo;
        private readonly IWorkshopService _workshopService;
        private readonly IActivityLogService _activityLogService;

        public JobTypeService(IRepository repo, IWorkshopService workshopService, IActivityLogService activityLogService)
        {
            _repo = repo;
            _workshopService = workshopService;
            _activityLogService = activityLogService;
        }

        public async Task<IEnumerable<JobTypeVM>> All(string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return new List<JobTypeVM>();

            return await _repo.GetAllAsNoTracking<JobType>()
                .Where(j => j.WorkshopId == workshopId)
                .Include(j => j.Workers)
                .Select(j => new JobTypeVM
                {
                    Id = j.Id,
                    Name = j.Name,
                    Description = j.Description,
                    Mechanics = j.Workers.Select(w => w.Name).ToList()
                })
                .ToListAsync();
        }

        public async Task Create(JobTypeVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var jobType = new JobType
            {
                Name = model.Name,
                Description = model.Description,
                WorkshopId = workshopId
            };

            if (model.Mechanics.Any())
            {
                var workers = await _repo.GetAll<Worker>()  // Remove AsNoTracking()
                    .Where(w => w.WorkshopId == workshopId && model.Mechanics.Contains(w.Name))
                    .ToListAsync();

                foreach (var worker in workers)
                {
                    jobType.Workers.Add(worker);  // EF will track the workers properly now
                }
            }

            await _repo.AddAsync(jobType);
            await _repo.SaveChangesAsync();

            await _activityLogService.LogActionAsync(userId, workshopId, "JobType",
                new ActivityLogData("created", jobType.Id, jobType.Name));
        }


        public async Task Delete(string id, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var jobType = await _repo.GetByIdAsync<JobType>(id);
            if (jobType == null) return;

            string name = jobType.Name;

            await _repo.DeleteAsync<JobType>(id);
            await _repo.SaveChangesAsync();

            await _activityLogService.LogActionAsync(userId, workshopId, "JobType",
                new ActivityLogData("deleted", null, name));
        }

        public async Task<JobTypeVM?> Details(string id)
        {
            return await _repo.GetAllAsNoTracking<JobType>()
                .Where(j => j.Id == id)
                .Include(j => j.Workers)
                .ThenInclude(w => w.User)
                .Select(j => new JobTypeVM
                {
                    Id = j.Id,
                    Name = j.Name,
                    Description = j.Description,
                    Mechanics = j.Workers.Select(w => w.Name).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task Edit(string id, JobTypeVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var jobType = await _repo.GetAll<JobType>()
                .Include(j => j.Workers)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobType != null)
            {
                var changes = new List<ActivityPropertyChange>();
                if (jobType.Name != model.Name)
                    changes.Add(new ActivityPropertyChange("name", jobType.Name, model.Name));
                if (jobType.Description != model.Description)
                    changes.Add(new ActivityPropertyChange("description", jobType.Description, model.Description));

                jobType.Name = model.Name;
                jobType.Description = model.Description;

                // Update workers
                var oldWorkerNames = jobType.Workers.Select(w => w.Name).ToList();
                var newWorkerNames = model.Mechanics ?? new List<string>();

                var addedWorkersNames = newWorkerNames.Except(oldWorkerNames).ToList();
                var removedWorkersNames = oldWorkerNames.Except(newWorkerNames).ToList();

                var allWorkshopWorkers = await _repo.GetAllAsNoTracking<Worker>()
                    .Where(w => w.WorkshopId == workshopId)
                    .Select(w => new { w.Id, w.Name })
                    .ToListAsync();

                foreach (var name in addedWorkersNames)
                {
                    var w = allWorkshopWorkers.FirstOrDefault(x => x.Name == name);
                    changes.Add(new ActivityPropertyChange("added worker", null, w?.Name ?? name, null, w?.Id));
                }
                foreach (var name in removedWorkersNames)
                {
                    var w = allWorkshopWorkers.FirstOrDefault(x => x.Name == name);
                    changes.Add(new ActivityPropertyChange("removed worker", w?.Name ?? name, null, w?.Id, null));
                }

                jobType.Workers.Clear();
                var workers = await _repo.GetAll<Worker>()
                    .Where(w => w.WorkshopId == workshopId && newWorkerNames.Contains(w.Name))
                    .ToListAsync();

                foreach (var worker in workers)
                {
                    jobType.Workers.Add(worker);
                }

                await _repo.SaveChangesAsync();

                if (changes.Count > 0)
                {
                    await _activityLogService.LogActionAsync(userId, workshopId, "JobType",
                        new ActivityLogData("updated", id, jobType.Name, Changes: changes));
                }
            }
        }
    }
}
