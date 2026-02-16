using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services.Jobs;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using GarageControl.Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using System;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Jobs;

namespace GarageControl.Tests.Services
{
    public class JobServiceTests
    {
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly GarageControlDbContext _context;
        private readonly JobService _service;

        public JobServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _mockInventoryService = new Mock<IInventoryService>();
            _mockAuthService = new Mock<IAuthService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            _service = new JobService(_context, _mockInventoryService.Object, _mockAuthService.Object, _mockActivityLogService.Object);
        }

        [Fact]
        public async Task GetMyJobsAsync_ShouldReturnJobsForWorker()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            
            var owner = new Client { Id = "c1", Name = "Client", WorkshopId = workshopId, PhoneNumber = "123-456-7890" };
            var make = new CarMake { Id = "m1", Name = "Toyota" };
            var model = new CarModel { Id = "mod1", Name = "Corolla", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "123", Model = model, Owner = owner };
            var order = new Order { Id = "o1", Car = car };
            var jobType = new JobType { Id = "jt1", Name = "Oil Change", WorkshopId = workshopId };
            var worker = new Worker { Id = "wkr1", UserId = userId, Name = "Worker", WorkshopId = workshopId };
            
            var job = new Job 
            { 
                Id = "j1", 
                Worker = worker, 
                Order = order, 
                JobType = jobType, 
                Status = JobStatus.Pending,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMyJobsAsync(userId, workshopId);

            // Assert
            Assert.Single(result);
            Assert.Equal("j1", result[0].Id);
            Assert.Equal("Toyota Corolla", result[0].CarName);
        }

        [Fact]
        public async Task CreateJobAsync_ShouldCreateAndLog()
        {
            // Arrange
            var userId = "u1";
            var orderId = "o1";
            var workshopId = "w1";

            var owner = new Client { Id = "c1", Name = "Client", WorkshopId = workshopId, PhoneNumber = "123-456-7890" };
            var make = new CarMake { Id = "m1", Name = "Toyota" };
            var model = new CarModel { Id = "mod1", Name = "Corolla", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "123", Model = model, Owner = owner };
            var order = new Order { Id = orderId, Car = car };
            var jobType = new JobType { Id = "jt1", Name = "Oil Change", WorkshopId = workshopId };
            var worker = new Worker { Id = "wkr1", UserId = userId, Name = "Worker", WorkshopId = workshopId };

            _context.Orders.Add(order);
            _context.JobTypes.Add(jobType);
            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            _mockAuthService.Setup(x => x.GetUserAccess(userId)).ReturnsAsync(new List<string> { "Parts Stock" });

            var createModel = new CreateJobVM 
            { 
                JobTypeId = "jt1", 
                WorkerId = "wkr1", 
                Status = JobStatus.Pending, 
                StartTime = DateTime.Now, 
                EndTime = DateTime.Now.AddHours(1),
                Parts = new List<CreateJobPartVM>()
            };

            // Act
            var result = await _service.CreateJobAsync(userId, orderId, workshopId, createModel);

            // Assert
            Assert.True(result.Success);
            var jobInDb = _context.Jobs.First();
            Assert.Equal("jt1", jobInDb.JobTypeId);

            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, It.Is<string>(s => s.Contains("created job"))), Times.Once);
        }
    }
}
