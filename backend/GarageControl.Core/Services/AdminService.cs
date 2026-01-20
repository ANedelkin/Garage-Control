using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageControl.Infrastructure.Data.Common;

namespace GarageControl.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository _repo;

        public AdminService(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IRepository repo)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _repo = repo;
        }

        public async Task<List<UserAdminVM>> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var workshops = await _repo.GetAllAsNoTrackingAsync<Workshop>().ToListAsync();
            var workers = await _repo.GetAllAsNoTrackingAsync<Worker>().Include(w => w.Workshop).ToListAsync();
            
            var userList = new List<UserAdminVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                string role = "Worker"; // Default to Worker if not Admin or Owner
                string? workshopName = null;

                if (roles.Contains("Admin"))
                {
                    role = "Admin";
                }
                else
                {
                    var worker = workers.FirstOrDefault(w => w.UserId == user.Id);
                    if (worker != null)
                    {
                        role = "Worker";
                        workshopName = worker.Workshop?.Name;
                    }
                    else
                    {
                        role = "Owner";
                        var ownerWorkshop = workshops.FirstOrDefault(w => w.BossId == user.Id);
                        workshopName = ownerWorkshop?.Name;
                    }
                }


                userList.Add(new UserAdminVM
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    IsBlocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    Role = role,
                    WorkshopName = workshopName
                });
            }

            return userList;
        }


        public async Task<MethodResponse> ToggleUserBlockAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new MethodResponse(false, "User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                return new MethodResponse(false, "Cannot block an admin user");
            }

            bool isCurrentlyBlocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

            if (isCurrentlyBlocked)
            {
                // Unblock
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                // Block for a long time (e.g., 100 years)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }

            return new MethodResponse(true, isCurrentlyBlocked ? "User unblocked successfully" : "User blocked successfully");
        }

        public async Task<List<WorkshopAdminVM>> GetWorkshopsAsync()
        {
            var workshops = await _repo.GetAllAsNoTrackingAsync<Workshop>()
                .Include(w => w.Boss)
                .Include(w => w.Workers)
                .ToListAsync();

            return workshops.Select(w => new WorkshopAdminVM
            {
                Id = w.Id,
                Name = w.Name,
                Address = w.Address,
                RegistrationNumber = w.RegistrationNumber,
                BossEmail = w.Boss?.Email ?? "",
                WorkerCount = w.Workers.Count,
                IsBlocked = w.IsBlocked
            }).ToList();
        }

        public async Task<MethodResponse> ToggleWorkshopBlockAsync(string workshopId)
        {
            var workshop = await _repo.GetAllAttachedAsync<Workshop>()
                .FirstOrDefaultAsync(w => w.Id == workshopId);

            if (workshop == null)
            {
                return new MethodResponse(false, "Workshop not found");
            }

            workshop.IsBlocked = !workshop.IsBlocked;
            await _repo.SaveChangesAsync();

            return new MethodResponse(true, workshop.IsBlocked ? "Workshop blocked successfully" : "Workshop unblocked successfully");
        }
    }
}

