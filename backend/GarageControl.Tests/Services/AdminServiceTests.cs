using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using System.Collections.Generic;
using System.Linq;
using GarageControl.Core.ViewModels;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IRepository> _mockRepo;
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);
            
            _mockRepo = new Mock<IRepository>();

            _service = new AdminService(_mockUserManager.Object, _mockRoleManager.Object, _mockRepo.Object);
        }

        [Fact]
        public async Task ToggleWorkshopBlockAsync_ShouldToggleAndSave()
        {
            // Arrange
            var workshopId = "w1";
            var workshop = new Workshop { Id = workshopId, IsBlocked = false };
            _mockRepo.Setup(x => x.GetByIdAsync<Workshop>(workshopId)).ReturnsAsync(workshop);

            // Act
            var result = await _service.ToggleWorkshopBlockAsync(workshopId);

            // Assert
            Assert.True(result.Success);
            Assert.True(workshop.IsBlocked);
            _mockRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ToggleUserBlockAsync_ShouldSetLockout()
        {
            // Arrange
            var userId = "u1";
            var user = new User { Id = userId };
            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.ToggleUserBlockAsync(userId);

            // Assert
            Assert.True(result.Success);
            _mockUserManager.Verify(x => x.SetLockoutEndDateAsync(user, It.Is<DateTimeOffset?>(d => d.HasValue)), Times.Once);
        }
    }
}
