using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
using GarageControl.Core.ViewModels.Workers;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using System.Globalization;
using GarageControl.Core.Helpers;
using GarageControl.Core.ViewModels.Shared;

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
             return await _repo.GetAllAsNoTracking<Access>()
                .Select(w => new AccessVM
                {
                    Id = w.Id,
                    Name = w.Name,
                    IsSelected = false
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkerVM>> All(string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return new List<WorkerVM>();

            var workers = await _repo.GetAllAsNoTracking<Worker>()
                .Where(w => w.WorkshopId == workshopId)
                .Include(w => w.User)
                .Include(w => w.Accesses)
                .Include(w => w.Activities)
                .Include(w => w.Schedules)
                //.Include(w => w.Leaves) // Assuming leaves need separate fetch or include if configured
                .AsSplitQuery()
                .ToListAsync();

            // Fetch leaves efficiently if not auto-included (or just ensure navigation property works)
            var allLeaves = await _repo.GetAllAsNoTracking<WorkerLeave>()
                .Where(l => workers.Select(w => w.Id).Contains(l.WorkerId))
                .ToListAsync();

            var allAccesses = await AllAccesses();

            return workers.Select(w => {
                var workerAccessIds = w.Accesses.Select(r => r.Id).ToList();
                var accessesVm = allAccesses.Select(r => new AccessVM 
                {
                    Id = r.Id, 
                    Name = r.Name, 
                    IsSelected = workerAccessIds.Contains(r.Id) 
                }).ToList();

                return new WorkerVM
                {
                Id = w.Id,
                Name = w.Name,
                Username = w.User.UserName!,
                Email = w.User.Email,
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
                    StartDate = l.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    EndDate = l.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                }).ToList(),
                Accesses = accessesVm
                };
            });
        }

        public async Task<MethodResponseVM> Create(WorkerVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null)
            {
                return new MethodResponseVM(false, "Workshop not found");
            }

            // Create a new user for the worker
            var newUser = new User
            {
                UserName = model.Username,
                Email = model.Email
            };

            var userCreationResult = await _userManager.CreateAsync(newUser, model.Password);
            if (!userCreationResult.Succeeded)
            {
                var errors = IdentityResultHelper.ProcessIdentityResult(userCreationResult);
                return new MethodResponseVM(false, "Failed to create user for worker", errors: errors);
            }

            // Create the worker and assign the new user's ID
            var worker = new Worker
            {
                Name = model.Name,
                HiredOn = model.HiredOn,
                WorkshopId = workshopId,
                UserId = newUser.Id
            };

            await _repo.AddAsync(worker);
            await _repo.SaveChangesAsync();

            // Store relations (accesses, job types, schedules, leaves)
            var changes = await UpdateWorkerRelations(worker.Id, model);

            await _activityLogService.LogActionAsync(userId, workshopId, "Worker",
                new ActivityLogData("created", worker.Id, worker.Name, Changes: changes.Any() ? changes : null));

            return new MethodResponseVM(true, "Worker created successfully");
        }

        public async Task Delete(string id, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var worker = await _repo.GetAllAttached<Worker>()
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
            var leaves = await _repo.GetAllAttached<WorkerLeave>()
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

            await _activityLogService.LogActionAsync(userId, workshopId, "Worker",
                new ActivityLogData("fired", null, workerName));
        }

        public async Task<WorkerVM?> Details(string id)
        {
            var worker = await _repo.GetAllAsNoTracking<Worker>()
                .Where(w => w.Id == id)
                .Include(w => w.User)
                .Include(w => w.Accesses)
                .Include(w => w.Schedules)
                .Include(w => w.Activities) // JobTypes
                .AsSplitQuery()
                .FirstOrDefaultAsync();
                
            if (worker == null) return null;

            // Fetch leaves separately if not navigable or lazy loading issue
            var leaves = await _repo.GetAllAsNoTracking<WorkerLeave>()
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
                Username = worker.User.UserName!,
                Email = worker.User.Email,
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
                    StartDate = l.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    EndDate = l.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                }).ToList()
            };
        }

        public async Task<MethodResponseVM> Edit(string id, WorkerVM model, string userId)
        {
            var worker = await _repo.GetByIdAsync<Worker>(id);
            if (worker == null)
            {
                return new MethodResponseVM(false, "Worker not found");
            }

            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null)
            {
                return new MethodResponseVM(false, "Workshop not found");
            }

            var user = await _userManager.FindByIdAsync(worker.UserId);
            if (user != null)
            {
                var changes = new List<ActivityPropertyChange>();
                void TrackChange(string fieldName, string? oldValue, string? newValue)
                {
                    if (oldValue != newValue)
                        changes.Add(new ActivityPropertyChange(fieldName, oldValue ?? "", newValue ?? ""));
                }

                TrackChange("name", worker.Name, model.Name);
                TrackChange("username", user.UserName, model.Username);
                TrackChange("email", user.Email, model.Email);
                TrackChange("hired date", worker.HiredOn.ToString("yyyy-MM-dd"), model.HiredOn.ToString("yyyy-MM-dd"));

                worker.Name = model.Name;

                bool userUpdated = false;

                if (user.UserName != model.Username)
                {
                    var existingUsername = await _userManager.FindByNameAsync(model.Username);
                    if (existingUsername != null)
                    {
                        return new MethodResponseVM(false, "Username is already taken");
                    }

                    user.UserName = model.Username;
                    userUpdated = true;
                }

                if (user.Email != model.Email)
                {
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var existingUser = await _userManager.FindByEmailAsync(model.Email);
                        if (existingUser != null)
                        {
                            return new MethodResponseVM(false, "Email is already taken");
                        }
                    }

                    user.Email = model.Email;
                    userUpdated = true;
                }

                if (userUpdated)
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = IdentityResultHelper.ProcessIdentityResult(result);
                        return new MethodResponseVM(false, "Failed to update user", errors: errors);
                    }
                }

                if (!string.IsNullOrEmpty(model.Password))
                {
                    var removeResult = await _userManager.RemovePasswordAsync(user);
                    if (!removeResult.Succeeded)
                    {
                        var errors = IdentityResultHelper.ProcessIdentityResult(removeResult);
                        return new MethodResponseVM(false, "Failed to remove password", errors: errors);
                    }

                    var addResult = await _userManager.AddPasswordAsync(user, model.Password);
                    if (!addResult.Succeeded)
                    {
                        var errors = IdentityResultHelper.ProcessIdentityResult(addResult);
                        return new MethodResponseVM(false, "Failed to add password", errors: errors);
                    }

                    changes.Add(new ActivityPropertyChange("password", "", "[changed]"));
                }

                worker.HiredOn = model.HiredOn;
                await _repo.SaveChangesAsync();

                var relationChanges = await UpdateWorkerRelations(worker.Id, model);
                changes.AddRange(relationChanges);

                if (changes.Count > 0)
                {
                    await _activityLogService.LogActionAsync(userId, workshopId, "Worker",
                        new ActivityLogData("updated", worker.Id, worker.Name, Changes: changes));
                }

                return new MethodResponseVM(true, "Worker updated successfully");
            }

            return new MethodResponseVM(false, "Unexpected error occurred");
        }
        
        private async Task<List<ActivityPropertyChange>> UpdateWorkerRelations(string workerId, WorkerVM model)
        {
            var changes = new List<ActivityPropertyChange>();
            var worker = await _repo.GetAllAttached<Worker>()
                .Where(w => w.Id == workerId)
                .Include(w => w.Accesses)
                .Include(w => w.Activities) // JobTypes
                .Include(w => w.Schedules)
                .Include(w => w.Leaves)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (worker == null) return changes;

            // Accesses
            var oldAccessIds = worker.Accesses.Select(r => r.Id).ToList();
            var newAccessIds = model.Accesses.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            
            var addedAccesses = model.Accesses.Where(r => r.IsSelected && !oldAccessIds.Contains(r.Id)).Select(r => r.Name).ToList();
            var removedAccesses = worker.Accesses.Where(r => !newAccessIds.Contains(r.Id)).Select(r => r.Name).ToList();
            
            foreach (var a in addedAccesses) changes.Add(new ActivityPropertyChange($"added access <b>{a}</b>", "", null));
            foreach (var a in removedAccesses) changes.Add(new ActivityPropertyChange($"removed access <b>{a}</b>", "", null));

            worker.Accesses.Clear();
            var accessesToAdd = await _repo.GetAllAttached<Access>().Where(r => newAccessIds.Contains(r.Id)).ToListAsync();
            foreach (var r in accessesToAdd) worker.Accesses.Add(r);

            // JobTypes (Activities)
            var oldJobTypeIds = worker.Activities.Select(j => j.Id).ToList();
            var newJobTypeIds = model.JobTypeIds ?? new List<string>();
            
            var addedJobTypes = await _repo.GetAllAsNoTracking<JobType>()
                .Where(j => newJobTypeIds.Contains(j.Id) && !oldJobTypeIds.Contains(j.Id))
                .Select(j => j.Name)
                .ToListAsync();
            var removedJobTypes = worker.Activities.Where(j => !newJobTypeIds.Contains(j.Id)).Select(j => j.Name).ToList();
            
            foreach (var j in addedJobTypes) changes.Add(new ActivityPropertyChange($"added job type <b>{j}</b>", "", null));
            foreach (var j in removedJobTypes) changes.Add(new ActivityPropertyChange($"removed job type <b>{j}</b>", "", null));

            worker.Activities.Clear();
            var jobTypesToAdd = await _repo.GetAllAttached<JobType>().Where(j => newJobTypeIds.Contains(j.Id)).ToListAsync();
            foreach (var j in jobTypesToAdd) worker.Activities.Add(j);

            // Schedules
            var oldSchedules = worker.Schedules.ToList();
            bool scheduleChanged = false;
            
            if (model.Schedules.Count != oldSchedules.Count)
            {
                scheduleChanged = true;
            }
            else
            {
                foreach(var s in model.Schedules)
                {
                    var backendDayOfWeek = (DayOfWeek)((s.DayOfWeek + 1) % 7);
                    var startTime = TimeOnly.Parse(s.StartTime);
                    var endTime = TimeOnly.Parse(s.EndTime);

                    var existing = oldSchedules.FirstOrDefault(os => os.DayOfWeek == backendDayOfWeek);
                    if (existing == null || existing.StartTime != startTime || existing.EndTime != endTime)
                    {
                        scheduleChanged = true;
                        break;
                    }
                }
            }

            if (scheduleChanged)
            {
                changes.Add(new ActivityPropertyChange("updated schedule", "", null));
                
                worker.Schedules.Clear();
                foreach(var s in model.Schedules)
                {
                    worker.Schedules.Add(new WorkerSchedule
                    {
                        WorkerId = workerId,
                        DayOfWeek = (DayOfWeek)((s.DayOfWeek + 1) % 7),
                        StartTime = TimeOnly.Parse(s.StartTime),
                        EndTime = TimeOnly.Parse(s.EndTime)
                    });
                }
            }

            // Leaves
            var oldLeaves = worker.Leaves.ToList();
            
            foreach(var l in model.Leaves)
            {
                var startDate = DateOnly.FromDateTime(l.StartDate.AddHours(12));
                var endDate = DateOnly.FromDateTime(l.EndDate.AddHours(12));
                
                var existing = oldLeaves.FirstOrDefault(ol => ol.StartDate == startDate && ol.EndDate == endDate);
                if (existing != null)
                {
                    oldLeaves.Remove(existing);
                }
                else
                {
                    changes.Add(new ActivityPropertyChange($"added leave from <b>{startDate:yyyy-MM-dd}</b> to <b>{endDate:yyyy-MM-dd}</b>", "", null));
                    await _repo.AddAsync(new WorkerLeave
                    {
                        WorkerId = workerId,
                        StartDate = startDate,
                        EndDate = endDate
                    });
                }
            }
            foreach (var ol in oldLeaves)
            {
                changes.Add(new ActivityPropertyChange($"deleted leave from <b>{ol.StartDate:yyyy-MM-dd}</b> to <b>{ol.EndDate:yyyy-MM-dd}</b>", "", null));
                _repo.Delete(ol);
            }

            await _repo.SaveChangesAsync();
            return changes;
        }

    }
}
