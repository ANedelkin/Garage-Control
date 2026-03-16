using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
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

            await _activityLogService.LogActionAsync(userId, workshopId, $"created Job Type <a href='/job-types?highlightId={jobType.Id}' class='log-link target-link'>{jobType.Name}</a>");
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

            await _activityLogService.LogActionAsync(userId, workshopId, $"deleted Job Type <b>{name}</b>");
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
                var changes = new List<string>();
                if (jobType.Name != model.Name)
                {
                    changes.Add($"name from <b>{jobType.Name}</b> to <b>{model.Name}</b>");
                }
                if (jobType.Description != model.Description)
                {
                    changes.Add("updated description");
                }

                jobType.Name = model.Name;
                jobType.Description = model.Description;

                // Update workers
                jobType.Workers.Clear();
                var workers = await _repo.GetAll<Worker>()  // Remove AsNoTracking()
                    .Where(w => w.WorkshopId == workshopId && model.Mechanics.Contains(w.Name))
                    .ToListAsync();

                foreach (var worker in workers)
                {
                    jobType.Workers.Add(worker);  // EF will track the workers properly now
                }

                await _repo.SaveChangesAsync();

                if (changes.Count > 0)
                {
                    string actionHtml;
                    if (changes.Count == 1 && changes[0].Contains("from"))
                    {
                        actionHtml = $"changed {changes[0]} of Job Type <a href='/job-types?highlightId={id}' class='log-link target-link'>{jobType.Name}</a>";
                    }
                    else if (changes.All(c => !c.Contains("from")))
                    {
                        actionHtml = $"updated details of Job Type <a href='/job-types?highlightId={id}' class='log-link target-link'>{jobType.Name}</a>";
                    }
                    else
                    {
                        actionHtml = $"updated Job Type <a href='/job-types?highlightId={id}' class='log-link target-link'>{jobType.Name}</a>: {string.Join(", ", changes)}";
                    }

                    await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
                }
            }
        }
    }
}
