using GarageControl.Core.Models;
using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using System.Collections.Generic;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
using GarageControl.Core.ViewModels.Workers;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Tests.Services
{
    public class WorkerServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly IRepository _repo;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly WorkerService _service;

        public WorkerServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _repo = new Repository(_context);
            
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            
            _mockWorkshopService = new Mock<IWorkshopService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            _service = new WorkerService(_repo, _mockUserManager.Object, _mockWorkshopService.Object, _mockActivityLogService.Object);
        }

        [Fact]
        public async Task Create_ShouldCreateUserWorkerAndLog()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var model = new WorkerVM 
            { 
                Name = "John", 
                Username = "john_worker",
                Email = "john@test.com", 
                Password = "Password123!", 
                HiredOn = DateTime.Now,
                Accesses = new List<AccessVM>(),
                Schedules = new List<WorkerScheduleVM>(),
                Leaves = new List<WorkerLeaveVM>()
            };

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), model.Password)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.Create(model, userId);

            // Assert
            _mockUserManager.Verify(x => x.CreateAsync(It.Is<User>(u => u.UserName == "john_worker" && u.Email == "john@test.com"), model.Password), Times.Once);
            var worker = await _context.Workers.FirstOrDefaultAsync(w => w.Name == "John");
            Assert.NotNull(worker);
            Assert.Equal(workshopId, worker.WorkshopId);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, "Worker", It.Is<ActivityLogData>(d => d.Action == "hired" && d.EntityName == "John")), Times.Once);
        }

        [Fact]
        public async Task Edit_ShouldUpdateUsernameAndEmail()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var identityUser = new User { Id = "iu1", UserName = "old_user", Email = "old@test.com" };
            var worker = new Worker { Id = "w1", Name = "John", WorkshopId = workshopId, UserId = "iu1", User = identityUser };
            
            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            var model = new WorkerVM 
            { 
                Id = "w1",
                Name = "John Updated", 
                Username = "new_user",
                Email = "new@test.com", 
                Accesses = new List<AccessVM>(),
                Schedules = new List<WorkerScheduleVM>(),
                Leaves = new List<WorkerLeaveVM>()
            };

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);
            _mockUserManager.Setup(x => x.FindByIdAsync("iu1")).ReturnsAsync(identityUser);
            _mockUserManager.Setup(x => x.FindByNameAsync("new_user")).ReturnsAsync((User)null);
            _mockUserManager.Setup(x => x.FindByEmailAsync("new@test.com")).ReturnsAsync((User)null);
            _mockUserManager.Setup(x => x.UpdateAsync(identityUser)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.Edit(model.Id!, model, userId);

            // Assert
            Assert.Equal("John Updated", worker.Name);
            Assert.Equal("new_user", identityUser.UserName);
            Assert.Equal("new@test.com", identityUser.Email);
            _mockUserManager.Verify(x => x.UpdateAsync(identityUser), Times.Once);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, "Worker", It.IsAny<ActivityLogData>()), Times.Once);
        }
    }
}
