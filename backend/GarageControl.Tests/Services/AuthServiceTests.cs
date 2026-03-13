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
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly IRepository _repo;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _repo = new Repository(_context);
            
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("ThisIsAVerySecretKeyForTestingPurposesOnly123!");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("GarageControl");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("GarageControlUsers");

            _service = new AuthService(_mockUserManager.Object, _repo, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task LogIn_ShouldReturnSuccessForValidCredentials()
        {
            // Arrange
            var model = new LoginVM { Username = "test", Password = "Pass" };
            var user = new User { Id = "u1", UserName = "test", Email = "test@test.com" };

            var workshop = new Workshop { Id = "w1", BossId = "u1", Name = "W", Address = "A", PhoneNumber = "123" };
            _context.Workshops.Add(workshop);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(x => x.FindByNameAsync(model.Username)).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Worker" });
            
            // Act
            var result = await _service.LogIn(model);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Token);
            Assert.Equal("u1", result.UserId);
        }

        [Fact]
        public async Task LogIn_ShouldReturnFailureForBlockedUser()
        {
            // Arrange
            var model = new LoginVM { Username = "test", Password = "Pass" };
            var user = new User { Id = "u1", UserName = "test", Email = "test@test.com", LockoutEnd = DateTimeOffset.UtcNow.AddDays(1) };

            _mockUserManager.Setup(x => x.FindByNameAsync(model.Username)).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
            
            // Act
            var result = await _service.LogIn(model);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("blocked", result.Message.ToLower());
        }
    }
}
