using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using GarageControl.Shared.Enums;
using System.Collections.Generic;
using System.Linq;

namespace GarageControl.Tests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<INotificationService> _mockNotification;
        private readonly GarageControlDbContext _context;
        private readonly InventoryService _service;

        public InventoryServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _mockNotification = new Mock<INotificationService>();

            _service = new InventoryService(_context, _mockNotification.Object);
        }

        [Fact]
        public async Task ApplyPartChangeAsync_ShouldSubtractQuantityAndBalance()
        {
            // Arrange
            var part = new Part { Quantity = 10, AvailabilityBalance = 10, Name = "Test" };

            // Act
            _service.SendParts(part, 3, JobStatus.InProgress);

            // Assert
            Assert.Equal(7, part.Quantity);
            Assert.Equal(7, part.AvailabilityBalance);
        }

        [Fact]
        public async Task ApplyPartChangeAsync_ShouldThrowIfInsufficientStock()
        {
            // Arrange
            var part = new Part { Quantity = 2, AvailabilityBalance = 2, Name = "Test" };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _service.SendParts(part, 3, JobStatus.InProgress));
        }

        [Fact]
        public async Task CheckLowStockAsync_ShouldNotifyIfBelowMinimum()
        {
            // Arrange
            var workshopId = "w1";
            var part = new Part { Id = "p1", Name = "Part", AvailabilityBalance = 1, MinimumQuantity = 2 };

            // Act
            await _service.CheckLowStockAsync(workshopId, part);

            // Assert
            _mockNotification.Verify(x => x.SendStockNotificationAsync(workshopId, "p1", "Part", 1, 2), Times.Once);
        }

        [Fact]
        public async Task GetPartsToSendAsync_ShouldSumCorrectly()
        {
            // Arrange
            var partId = "p1";
            var jobType = new JobType { Id = "jt1", Name = "Test", WorkshopId = "w1" };
            var worker = new Worker { Id = "wkr1", Name = "Worker", WorkshopId = "w1", UserId = "u1" };
            var client = new Client { Id = "c1", Name = "Client", WorkshopId = "w1", PhoneNumber = "123" };
            var make = new CarMake { Id = "m1", Name = "Make" };
            var model = new CarModel { Id = "mod1", Name = "Model", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "REG", Model = model, Owner = client };
            var order1 = new Order { Id = "o1", Car = car };
            var order2 = new Order { Id = "o2", Car = car };
            
            _context.JobTypes.Add(jobType);
            _context.Workers.Add(worker);
            _context.Orders.AddRange(order1, order2);
            await _context.SaveChangesAsync();
            
            var job1 = new Job { Id = "j1", Status = JobStatus.Pending, JobTypeId = "jt1", WorkerId = "wkr1", OrderId = "o1", StartTime = DateTime.Now, EndTime = DateTime.Now };
            var job2 = new Job { Id = "j2", Status = JobStatus.Pending, JobTypeId = "jt1", WorkerId = "wkr1", OrderId = "o2", StartTime = DateTime.Now, EndTime = DateTime.Now };
            _context.Jobs.AddRange(job1, job2);
            await _context.SaveChangesAsync();
            
            var jp1 = new JobPart { JobId = "j1", PartId = partId, PlannedQuantity = 5, SentQuantity = 2 }; // to send: 3
            var jp2 = new JobPart { JobId = "j2", PartId = partId, PlannedQuantity = 10, SentQuantity = 10 }; // to send: 0
            
            _context.JobParts.AddRange(jp1, jp2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetPartsToSendAsync(partId);

            // Assert
            Assert.Equal(3, result);
        }
    }
}
