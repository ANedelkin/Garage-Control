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
using GarageControl.Core.ViewModels.Jobs;
using Microsoft.EntityFrameworkCore;
using System;

namespace GarageControl.Tests.Services
{
    public class JobTypeServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly IRepository _repo;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly JobTypeService _service;

        public JobTypeServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _repo = new Repository(_context);
            _mockWorkshopService = new Mock<IWorkshopService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            _service = new JobTypeService(_repo, _mockWorkshopService.Object, _mockActivityLogService.Object);
        }

        [Fact]
        public async Task Create_ShouldAddJobTypeAndLog()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var model = new JobTypeVM { Name = "Oil Change" };

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);

            // Act
            await _service.Create(model, userId);

            // Assert
            var jobType = await _context.JobTypes.FirstOrDefaultAsync(jt => jt.Name == "Oil Change");
            Assert.NotNull(jobType);
            Assert.Equal(workshopId, jobType.WorkshopId);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, "JobType", It.IsAny<ActivityLogData>()), Times.Once);
        }

        [Fact]
        public async Task All_ShouldReturnJobTypesForWorkshop()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            
            _context.JobTypes.Add(new JobType { Id = "jt1", Name = "Oil Change", WorkshopId = workshopId });
            _context.JobTypes.Add(new JobType { Id = "jt2", Name = "Tire Rotation", WorkshopId = "w2" });
            await _context.SaveChangesAsync();

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);

            // Act
            var result = await _service.All(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Oil Change", result.First().Name);
        }
    }
}
