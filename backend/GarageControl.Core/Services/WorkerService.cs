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
        
        public WorkerService(IRepository repo, UserManager<User> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        public async Task<IEnumerable<RoleVM>> AllRoles()
        {
             return await _repo.GetAllAsNoTrackingAsync<Role>()
                .Select(r => new RoleVM
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = false
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkerVM>> All(string userId)
        {
            var serviceId = (await _repo.GetAllAsNoTrackingAsync<CarService>().Where(s => s.BossId == userId).FirstOrDefaultAsync())?.Id;
            if (serviceId == null) return new List<WorkerVM>();

            var workers = await _repo.GetAllAsNoTrackingAsync<Worker>()
                .Where(w => w.CarServiceId == serviceId)
                .Include(w => w.User)
                .Include(w => w.Roles)
                .ToListAsync();

            return workers.Select(w => new WorkerVM
            {
                Id = w.Id,
                FirstName = "FixMeSplitNames", // Simplification as User model doesn't have split names in snippet
                LastName = w.User.UserName!,
                Email = w.User.Email!,
                HiredOn = w.HiredOn,
                // Simplified list view, detail view will have full data
            });
        }

        public async Task Create(WorkerVM model, string userId)
        {
            var serviceId = (await _repo.GetAllAsNoTrackingAsync<CarService>().Where(s => s.BossId == userId).FirstOrDefaultAsync())?.Id;
            if (serviceId == null) throw new ArgumentException("User does not have a service");

            // 1. Create Identity User
            var user = new User
            {
                UserName = $"{model.FirstName} {model.LastName}",
                Email = model.Email,
                EmailConfirmed = true 
            };
            
            var result = await _userManager.CreateAsync(user, model.Password ?? "Default123!");
            if (!result.Succeeded)
            {
                 throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // 2. Create Worker Entity
            var worker = new Worker
            {
                UserId = user.Id,
                CarServiceId = serviceId,
                HiredOn = model.HiredOn
            };
            
            await _repo.AddAsync(worker);
            await _repo.SaveChangesAsync();

            // 3. Add Relations (Roles, Schedules, Leaves, JobTypes)
            await UpdateWorkerRelations(worker.Id, model);
        }

        public async Task Delete(string id)
        {
            // Complex delete: remove worker, schedules, leaves, and potentially the user account
            // For now, simple implementation
             await _repo.DeleteAsync<Worker>(id);
             await _repo.SaveChangesAsync();
        }

        public async Task<WorkerVM?> Details(string id)
        {
            var worker = await _repo.GetAllAsNoTrackingAsync<Worker>()
                .Where(w => w.Id == id)
                .Include(w => w.User)
                .Include(w => w.Roles)
                .Include(w => w.Schedules)
                .Include(w => w.Activities) // JobTypes
                .FirstOrDefaultAsync();
                
            if (worker == null) return null;

            // Fetch leaves separately if not navigable or lazy loading issue
            var leaves = await _repo.GetAllAsNoTrackingAsync<WorkerLeave>()
                .Where(l => l.WorkerId == id)
                .ToListAsync();

            var allRoles = await AllRoles();
            var workerRoleIds = worker.Roles.Select(r => r.Id).ToList();
            
            var rolesVm = allRoles.Select(r => new RoleVM 
            {
                Id = r.Id, 
                Name = r.Name, 
                IsSelected = workerRoleIds.Contains(r.Id) 
            }).ToList();

            var nameParts = worker.User.UserName?.Split(' ') ?? new string[] {"Unknown", "Unknown"};

            return new WorkerVM
            {
                Id = worker.Id,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                Email = worker.User.Email!,
                HiredOn = worker.HiredOn,
                Roles = rolesVm,
                JobTypeIds = worker.Activities.Select(a => a.Id).ToList(),
                Schedules = worker.Schedules.Select(s => new WorkerScheduleVM
                {
                    Id = s.Id,
                    DayOfWeek = (int)s.DayOfWeek,
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

        public async Task Edit(WorkerVM model)
        {
            var worker = await _repo.GetByIdAsync<Worker>(model.Id!);
            if (worker == null) return;
            
            var user = await _userManager.FindByIdAsync(worker.UserId);
            if (user != null)
            {
                user.UserName = $"{model.FirstName} {model.LastName}";
                user.Email = model.Email;
                await _userManager.UpdateAsync(user);
                
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.Password);
                }
            }

            worker.HiredOn = model.HiredOn;
            await _repo.SaveChangesAsync();

            await UpdateWorkerRelations(worker.Id, model);
        }
        
        private async Task UpdateWorkerRelations(string workerId, WorkerVM model)
        {
            var worker = await _repo.GetAllAttachedAsync<Worker>()
                .Where(w => w.Id == workerId)
                .Include(w => w.Roles)
                .Include(w => w.Activities) // JobTypes
                .Include(w => w.Schedules)
                .FirstOrDefaultAsync();

            if (worker == null) return;

            // Roles
            worker.Roles.Clear();
            var selectedRoleIds = model.Roles.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            var rolesToAdd = await _repo.GetAllAsNoTrackingAsync<Role>().Where(r => selectedRoleIds.Contains(r.Id)).ToListAsync();
            foreach (var r in rolesToAdd) worker.Roles.Add(r);

            // JobTypes (Activities)
            worker.Activities.Clear();
            var jobTypesToAdd = await _repo.GetAllAsNoTrackingAsync<JobType>().Where(j => model.JobTypeIds.Contains(j.Id)).ToListAsync();
            foreach (var j in jobTypesToAdd) worker.Activities.Add(j);

            // Schedules
            // Ideally should update existing ones, but replace strategy prevents stale data
            // We need to fetch attached schedules to remove them
            var schedulesToRemove = await _repo.GetAllAttachedAsync<WorkerSchedule>().Where(s => s.WorkerId == workerId).ToListAsync();
            foreach(var s in schedulesToRemove) _repo.Delete(s);
            
            foreach(var s in model.Schedules)
            {
               await _repo.AddAsync(new WorkerSchedule
               {
                   WorkerId = workerId,
                   DayOfWeek = (DayOfWeek)s.DayOfWeek,
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
