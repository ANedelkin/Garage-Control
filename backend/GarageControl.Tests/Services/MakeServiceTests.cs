using GarageControl.Core.Models;
using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using System.Linq;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;
using Microsoft.EntityFrameworkCore;
using System;

namespace GarageControl.Tests.Services
{
    public class MakeServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly IRepository _repo;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly MakeService _service;

        public MakeServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _repo = new Repository(_context);
            _mockWorkshopService = new Mock<IWorkshopService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            _service = new MakeService(_repo, _mockWorkshopService.Object, _mockActivityLogService.Object);
        }

        [Fact]
        public async Task CreateMake_ShouldAddCustomMakeAndLog()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var model = new MakeVM { Name = "CustomMake" };

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);
            _mockWorkshopService.Setup(x => x.GetWorkshopBossId(userId)).ReturnsAsync(userId);

            // Act
            await _service.CreateMake(model, userId);

            // Assert
            var make = await _context.CarMakes.FirstOrDefaultAsync(m => m.Name == "CustomMake");
            Assert.NotNull(make);
            Assert.Equal(userId, make.CreatorId);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, "Make", It.Is<ActivityLogData>(d => d.Action == "created")), Times.Once);
        }

        [Fact]
        public async Task GetMakes_ShouldReturnGlobalAndCustomMakes()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            
            _context.CarMakes.Add(new CarMake { Id = "m1", Name = "GlobalMake", CreatorId = null });
            _context.CarMakes.Add(new CarMake { Id = "m2", Name = "CustomMake", CreatorId = userId });
            _context.CarMakes.Add(new CarMake { Id = "m3", Name = "OtherMake", CreatorId = "other" });
            await _context.SaveChangesAsync();

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);
            _mockWorkshopService.Setup(x => x.GetWorkshopBossId(userId)).ReturnsAsync(userId);

            // Act
            var result = await _service.GetMakes(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, m => m.Name == "GlobalMake");
            Assert.Contains(result, m => m.Name == "CustomMake");
        }
    }
}
