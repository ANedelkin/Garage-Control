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
                $"hired <a href='/workers/{worker.Id}' class='log-link target-link'>{worker.Name}</a>");
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
                $"fired <b>{workerName}</b>");
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
                var changes = new List<string>();
                void TrackChange(string fieldName, object? oldValue, object? newValue)
                {
                    string oldStr = oldValue?.ToString() ?? "";
                    string newStr = newValue?.ToString() ?? "";
                    if (oldStr != newStr)
                    {
                        string oldDisp = string.IsNullOrEmpty(oldStr) ? "[empty]" : oldStr;
                        string newDisp = string.IsNullOrEmpty(newStr) ? "[empty]" : newStr;
                        
                        if (oldDisp.Length > 100 || newDisp.Length > 100)
                        {
                            changes.Add(fieldName);
                        }
                        else
                        {
                            changes.Add($"{fieldName} from <b>{oldDisp}</b> to <b>{newDisp}</b>");
                        }
                    }
                }

                TrackChange("name", worker.Name, model.Name);
                TrackChange("email", user.Email, model.Email);
                TrackChange("hired date", worker.HiredOn.ToString("yyyy-MM-dd"), model.HiredOn.ToString("yyyy-MM-dd"));

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
                    changes.Add("password");
                }

                worker.HiredOn = model.HiredOn;
                await _repo.SaveChangesAsync();

                var relationChanges = await UpdateWorkerRelations(worker.Id, model);
                changes.AddRange(relationChanges);

                if (changes.Count > 0)
                {
                    string workerLink = $"<a href='/workers/{worker.Id}' class='log-link target-link'>{worker.Name}</a>";
                    string actionHtml;

                    if (changes.Count == 1 && (changes[0].Contains("from") || changes[0].Contains("Updated schedule")))
                    {
                        actionHtml = $"changed {changes[0]} of worker {workerLink}";
                    }
                    else if (changes.All(c => !c.Contains("from") && !c.Contains("added") && !c.Contains("removed") && !c.Contains("Updated schedule") && !c.Contains("deleted")))
                    {
                        actionHtml = $"updated details of worker {workerLink}";
                    }
                    else
                    {
                        actionHtml = $"updated worker {workerLink}: {string.Join(", ", changes)}";
                    }

                    await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
                }
            }
        }
        
        private async Task<List<string>> UpdateWorkerRelations(string workerId, WorkerVM model)
        {
            var changes = new List<string>();
            var worker = await _repo.GetAllAttachedAsync<Worker>()
                .Where(w => w.Id == workerId)
                .Include(w => w.Accesses)
                .Include(w => w.Activities) // JobTypes
                .Include(w => w.Schedules)
                .Include(w => w.Leaves)
                .FirstOrDefaultAsync();

            if (worker == null) return changes;

            // Accesses
            var oldAccessIds = worker.Accesses.Select(r => r.Id).ToList();
            var newAccessIds = model.Accesses.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            
            var addedAccesses = model.Accesses.Where(r => r.IsSelected && !oldAccessIds.Contains(r.Id)).Select(r => r.Name).ToList();
            var removedAccesses = worker.Accesses.Where(r => !newAccessIds.Contains(r.Id)).Select(r => r.Name).ToList();
            
            foreach (var a in addedAccesses) changes.Add($"added access <b>{a}</b>");
            foreach (var a in removedAccesses) changes.Add($"removed access <b>{a}</b>");

            worker.Accesses.Clear();
            var accessesToAdd = await _repo.GetAllAttachedAsync<Access>().Where(r => newAccessIds.Contains(r.Id)).ToListAsync();
            foreach (var r in accessesToAdd) worker.Accesses.Add(r);

            // JobTypes (Activities)
            var oldJobTypeIds = worker.Activities.Select(j => j.Id).ToList();
            var newJobTypeIds = model.JobTypeIds ?? new List<string>();
            
            var addedJobTypes = await _repo.GetAllAsNoTrackingAsync<JobType>()
                .Where(j => newJobTypeIds.Contains(j.Id) && !oldJobTypeIds.Contains(j.Id))
                .Select(j => j.Name)
                .ToListAsync();
            var removedJobTypes = worker.Activities.Where(j => !newJobTypeIds.Contains(j.Id)).Select(j => j.Name).ToList();
            
            foreach (var j in addedJobTypes) changes.Add($"added job type <b>{j}</b>");
            foreach (var j in removedJobTypes) changes.Add($"removed job type <b>{j}</b>");

            worker.Activities.Clear();
            var jobTypesToAdd = await _repo.GetAllAttachedAsync<JobType>().Where(j => newJobTypeIds.Contains(j.Id)).ToListAsync();
            foreach (var j in jobTypesToAdd) worker.Activities.Add(j);

            // Schedules
            var oldSchedules = worker.Schedules.ToList();
            worker.Schedules.Clear();
            
            foreach(var s in model.Schedules)
            {
               var backendDayOfWeek = (DayOfWeek)((s.DayOfWeek + 1) % 7);
               var startTime = TimeOnly.Parse(s.StartTime);
               var endTime = TimeOnly.Parse(s.EndTime);

               var existing = oldSchedules.FirstOrDefault(os => os.DayOfWeek == backendDayOfWeek);
               if (existing != null)
               {
                   if (existing.StartTime != startTime || existing.EndTime != endTime)
                   {
                       changes.Add($"Updated schedule for <b>{backendDayOfWeek}</b> from <b>{existing.StartTime:HH:mm}-{existing.EndTime:HH:mm}</b> to <b>{startTime:HH:mm}-{endTime:HH:mm}</b>");
                   }
                   oldSchedules.Remove(existing);
               }
               else
               {
                   changes.Add($"added schedule for <b>{backendDayOfWeek}</b>: <b>{startTime:HH:mm}-{endTime:HH:mm}</b>");
               }

               worker.Schedules.Add(new WorkerSchedule
               {
                   WorkerId = workerId,
                   DayOfWeek = backendDayOfWeek,
                   StartTime = startTime,
                   EndTime = endTime
               });
            }
            foreach (var os in oldSchedules)
            {
                changes.Add($"removed schedule for <b>{os.DayOfWeek}</b>");
            }

            // Leaves
            var oldLeaves = worker.Leaves.ToList();
            
            foreach(var l in model.Leaves)
            {
                var startDate = DateOnly.FromDateTime(l.StartDate);
                var endDate = DateOnly.FromDateTime(l.EndDate);
                
                var existing = oldLeaves.FirstOrDefault(ol => ol.StartDate == startDate && ol.EndDate == endDate);
                if (existing != null)
                {
                    oldLeaves.Remove(existing);
                }
                else
                {
                    changes.Add($"added leave from <b>{startDate:yyyy-MM-dd}</b> to <b>{endDate:yyyy-MM-dd}</b>");
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
                changes.Add($"deleted leave from <b>{ol.StartDate:yyyy-MM-dd}</b> to <b>{ol.EndDate:yyyy-MM-dd}</b>");
                _repo.Delete(ol);
            }

            await _repo.SaveChangesAsync();
            return changes;
        }

    }
}
