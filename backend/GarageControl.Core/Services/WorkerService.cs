using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using System.Globalization;

namespace GarageControl.Core.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly IRepository _repo;
        private readonly UserManager<User> _userManager;
        private readonly IWorkshopService _workshopService;
        private readonly IActivityLogService _activityLogService;
        
        public WorkerService(IRepository repo, UserManager<User> userManager, IWorkshopService workshopService, IActivityLogService activityLogService)
        {
            _repo = repo;
            _userManager = userManager;
            _workshopService = workshopService;
            _activityLogService = activityLogService;
        }

        public async Task<IEnumerable<AccessVM>> AllAccesses()
        {
             return await _repo.GetAllAsNoTrackingAsync<Access>()
                .Select(r => new AccessVM
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = false
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkerVM>> All(string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return new List<WorkerVM>();

            var workers = await _repo.GetAllAsNoTrackingAsync<Worker>()
                .Where(w => w.WorkshopId == workshopId)
                .Include(w => w.User)
                .Include(w => w.Accesses)
                .Include(w => w.Activities)
                .Include(w => w.Schedules)
                //.Include(w => w.Leaves) // Assuming leaves need separate fetch or include if configured
                .ToListAsync();

            // Fetch leaves efficiently if not auto-included (or just ensure navigation property works)
            var allLeaves = await _repo.GetAllAsNoTrackingAsync<WorkerLeave>()
                .Where(l => workers.Select(w => w.Id).Contains(l.WorkerId))
                .ToListAsync();

            return workers.Select(w => new WorkerVM
            {
                Id = w.Id,
                Name = w.Name,
                Email = w.User.Email!,
                HiredOn = w.HiredOn,
                JobTypeIds = w.Activities.Select(a => a.Id).ToList(),
                Schedules = w.Schedules.Select(s => new WorkerScheduleVM
                {
                    Id = s.Id,
                    DayOfWeek = (int)s.DayOfWeek == 0 ? 6 : (int)s.DayOfWeek - 1,
                    StartTime = s.StartTime.ToString("HH:mm"),
                    EndTime = s.EndTime.ToString("HH:mm")
                }).ToList(),
                Leaves = allLeaves.Where(l => l.WorkerId == w.Id).Select(l => new WorkerLeaveVM
                {
                    Id = l.Id,
                    StartDate = l.StartDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = l.EndDate.ToDateTime(TimeOnly.MinValue)
                }).ToList()
            });
        }

        public async Task Create(WorkerVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            // 1. Create Identity User
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true 
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                 throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // 2. Create Worker Entity
            var worker = new Worker
            {
                UserId = user.Id,
                Name = model.Name,
                WorkshopId = workshopId,
                HiredOn = model.HiredOn
            };
            
            await _repo.AddAsync(worker);
            await _repo.SaveChangesAsync();

            // 3. Add Relations (Roles, Schedules, Leaves, JobTypes)
            await UpdateWorkerRelations(worker.Id, model);

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                "hired",
                worker.Id,
                worker.Name,
                "Worker");
        }

        public async Task Delete(string id, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var worker = await _repo.GetAllAttachedAsync<Worker>()
                .Where(w => w.Id == id)
                .Include(w => w.Schedules)
                .Include(w => w.Leaves) // Assuming navigation property exists or we manually delete
                .FirstOrDefaultAsync();

            if (worker == null) return;

            string workerName = worker.Name;

            // 1. Delete Schedules
            if (worker.Schedules != null && worker.Schedules.Any())
            {
                // We can remove them from the collection or delete directly
                // Helper loop to clear list if tracking is tricky, or just delete from repo
                var schedules = worker.Schedules.ToList();
                foreach (var s in schedules)
                {
                    _repo.Delete(s);
                }
            }

            // 2. Delete Leaves
            // Fetch if not included (if navigation prop missing in previous context, assumed manual fetch in original code)
            var leaves = await _repo.GetAllAttachedAsync<WorkerLeave>()
                .Where(l => l.WorkerId == id)
                .ToListAsync();
            
            foreach(var l in leaves)
            {
                _repo.Delete(l);
            }

            // 3. Delete Worker (Roles/Activities handle themselves usually via join tables or explicit cleanup if needed)
            // But usually ManyToMany join entities are handled by EF if configured correctly. 
            // If they are separate entities, we might need to clear them too.
            // Assuming standard scaffold:
            _repo.Delete(worker);
            
            await _repo.SaveChangesAsync();

            // 4. Delete Identity User
            if (!string.IsNullOrEmpty(worker.UserId))
            {
                var user = await _userManager.FindByIdAsync(worker.UserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                "fired",
                null,
                workerName,
                "Worker");
        }

        public async Task<WorkerVM?> Details(string id)
        {
            var worker = await _repo.GetAllAsNoTrackingAsync<Worker>()
                .Where(w => w.Id == id)
                .Include(w => w.User)
                .Include(w => w.Accesses)
                .Include(w => w.Schedules)
                .Include(w => w.Activities) // JobTypes
                .FirstOrDefaultAsync();
                
            if (worker == null) return null;

            // Fetch leaves separately if not navigable or lazy loading issue
            var leaves = await _repo.GetAllAsNoTrackingAsync<WorkerLeave>()
                .Where(l => l.WorkerId == id)
                .ToListAsync();

            var allAccesses = await AllAccesses();
            var workerAccessIds = worker.Accesses.Select(r => r.Id).ToList();
            
            var accessesVm = allAccesses.Select(r => new AccessVM 
            {
                Id = r.Id, 
                Name = r.Name, 
                IsSelected = workerAccessIds.Contains(r.Id) 
            }).ToList();

            var nameParts = worker.User.UserName?.Split(' ') ?? new string[] {"Unknown", "Unknown"};

            return new WorkerVM
            {
                Id = worker.Id,
                Name = worker.Name,
                Email = worker.User.Email!,
                HiredOn = worker.HiredOn,
                Accesses = accessesVm,
                JobTypeIds = worker.Activities.Select(a => a.Id).ToList(),
                Schedules = worker.Schedules.Select(s => new WorkerScheduleVM
                {
                    Id = s.Id,
                    // Map Backend DayOfWeek (0=Sun, 1=Mon) to Frontend Day (0=Mon, 6=Sun)
                    DayOfWeek = (int)s.DayOfWeek == 0 ? 6 : (int)s.DayOfWeek - 1,
                    StartTime = s.StartTime.ToString("HH:mm"),
                    EndTime = s.EndTime.ToString("HH:mm")
                }).ToList(),
                Leaves = leaves.Select(l => new WorkerLeaveVM
                {
                    Id = l.Id,
                    StartDate = l.StartDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = l.EndDate.ToDateTime(TimeOnly.MinValue)
                }).ToList()
            };
        }

        public async Task Edit(WorkerVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var worker = await _repo.GetByIdAsync<Worker>(model.Id!);
            if (worker == null) return;
            
            var user = await _userManager.FindByIdAsync(worker.UserId);
            if (user != null)
            {
                worker.Name = model.Name;
                
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        throw new Exception("Email is already taken");
                    }

                    user.Email = model.Email;
                    user.UserName = model.Email;
                    
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.Password);
                }
            }

            worker.HiredOn = model.HiredOn;
            await _repo.SaveChangesAsync();

            await UpdateWorkerRelations(worker.Id, model);

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                "updated",
                worker.Id,
                worker.Name,
                "Worker");
        }
        
        private async Task UpdateWorkerRelations(string workerId, WorkerVM model)
        {
            var worker = await _repo.GetAllAttachedAsync<Worker>()
                .Where(w => w.Id == workerId)
                .Include(w => w.Accesses)
                .Include(w => w.Activities) // JobTypes
                .Include(w => w.Schedules)
                .FirstOrDefaultAsync();

            if (worker == null) return;

            // Accesses
            worker.Accesses.Clear();
            var selectedAccessIds = model.Accesses.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            var accessesToAdd = await _repo.GetAllAttachedAsync<Access>().Where(r => selectedAccessIds.Contains(r.Id)).ToListAsync();
            foreach (var r in accessesToAdd) worker.Accesses.Add(r);

            // JobTypes (Activities)
            worker.Activities.Clear();
            var jobTypesToAdd = await _repo.GetAllAttachedAsync<JobType>().Where(j => model.JobTypeIds.Contains(j.Id)).ToListAsync();
            foreach (var j in jobTypesToAdd) worker.Activities.Add(j);

            // Schedules
            // Update via collection to ensure change tracking works correctly with the attached worker entity
            worker.Schedules.Clear();
            
            foreach(var s in model.Schedules)
            {
               // Map Frontend Day (0=Mon) to Backend DayOfWeek (0=Sun, 1=Mon)
               var backendDayOfWeek = (DayOfWeek)((s.DayOfWeek + 1) % 7);

               worker.Schedules.Add(new WorkerSchedule
               {
                   WorkerId = workerId,
                   DayOfWeek = backendDayOfWeek,
                   StartTime = TimeOnly.Parse(s.StartTime),
                   EndTime = TimeOnly.Parse(s.EndTime)
               });
            }

            // Leaves
            var leavesToRemove = await _repo.GetAllAttachedAsync<WorkerLeave>().Where(l => l.WorkerId == workerId).ToListAsync();
            foreach(var l in leavesToRemove) _repo.Delete(l);
            
            foreach(var l in model.Leaves)
            {
                await _repo.AddAsync(new WorkerLeave
                {
                    WorkerId = workerId,
                    StartDate = DateOnly.FromDateTime(l.StartDate),
                    EndDate = DateOnly.FromDateTime(l.EndDate)
                });
            }

            await _repo.SaveChangesAsync();
        }

    }
}
