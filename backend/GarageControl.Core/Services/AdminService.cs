using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
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
        public async Task<MethodResponseVM> ToggleUserBlockAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new MethodResponseVM(false, "User not found");
            }

            if (user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.UtcNow)
            {
                // Block for 100 years
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                return new MethodResponseVM(true, "User blocked successfully");
            }
            else
            {
                // Unblock
                await _userManager.SetLockoutEndDateAsync(user, null);
                return new MethodResponseVM(true, "User unblocked successfully");
            }
        }

        public async Task<List<WorkshopAdminVM>> GetWorkshopsAsync()
        {
            var workshops = await _repo.GetAllAsNoTrackingAsync<Workshop>().ToListAsync();
            var users = await _userManager.Users.ToListAsync();

            return workshops.Select(w => new WorkshopAdminVM
            {
                Id = w.Id,
                Name = w.Name,
                Address = w.Address,
                RegistrationNumber = w.RegistrationNumber,
                OwnerEmail = users.FirstOrDefault(u => u.Id == w.BossId)?.Email ?? "N/A",
                IsBlocked = w.IsBlocked
            }).ToList();
        }

        public async Task<MethodResponseVM> ToggleWorkshopBlockAsync(string workshopId)
        {
            var workshop = await _repo.GetByIdAsync<Workshop>(workshopId);
            if (workshop == null)
            {
                return new MethodResponseVM(false, "Workshop not found");
            }

            workshop.IsBlocked = !workshop.IsBlocked;
            await _repo.SaveChangesAsync();

            return new MethodResponseVM(true, workshop.IsBlocked ? "Workshop blocked successfully" : "Workshop unblocked successfully");
        }

        public async Task<DashboardStatsVM> GetDashboardStatsAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalWorkshops = await _repo.GetAllAsNoTrackingAsync<Workshop>().CountAsync();
            var totalOrders = await _repo.GetAllAsNoTrackingAsync<Order>().CountAsync();

            var recentUsersList = await _userManager.Users
                .OrderByDescending(u => u.Id) // Assuming Id is vaguely time ordered or just taking any
                .Take(5)
                .ToListAsync();

            var recentUsersVM = new List<UserAdminVM>();
            var workshops = await _repo.GetAllAsNoTrackingAsync<Workshop>().ToListAsync();
            var workers = await _repo.GetAllAsNoTrackingAsync<Worker>().Include(w => w.Workshop).ToListAsync();

            foreach (var user in recentUsersList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                string role = "Worker";
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

                recentUsersVM.Add(new UserAdminVM
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    IsBlocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    Role = role,
                    WorkshopName = workshopName
                });
            }

            return new DashboardStatsVM
            {
                TotalUsers = totalUsers,
                TotalWorkshops = totalWorkshops,
                TotalOrders = totalOrders,
                RecentUsers = recentUsersVM
            };
        }
    }
}

