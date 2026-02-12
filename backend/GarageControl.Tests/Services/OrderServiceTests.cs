using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using GarageControl.Core.ViewModels;

namespace GarageControl.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly GarageControlDbContext _context;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _mockNotificationService = new Mock<INotificationService>();
            _mockActivityLogService = new Mock<IActivityLogService>();
            _mockWorkshopService = new Mock<IWorkshopService>();
            _mockInventoryService = new Mock<IInventoryService>();

            _service = new OrderService(
                _context, 
                _mockNotificationService.Object, 
                _mockActivityLogService.Object, 
                _mockWorkshopService.Object, 
                _mockInventoryService.Object);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldReturnOrdersForWorkshop()
        {
            // Arrange
            var workshopId = "w1";
            var owner = new Client { Id = "c1", Name = "Client", WorkshopId = workshopId, PhoneNumber = "123-456-7890" };
            var make = new CarMake { Id = "m1", Name = "Toyota" };
            var model = new CarModel { Id = "mod1", Name = "Corolla", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "123", Model = model, Owner = owner };
            var order = new Order { Id = "o1", Car = car, Kilometers = 1000, IsDone = false };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetOrdersAsync(workshopId);

            // Assert
            Assert.Single(result);
            Assert.Equal("o1", result[0].Id);
            Assert.Equal("Client", result[0].ClientName);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateAndLog()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var owner = new Client { Id = "c1", Name = "Client", WorkshopId = workshopId, PhoneNumber = "123-456-7890" };
            var make = new CarMake { Id = "m1", Name = "Toyota" };
            var model = new CarModel { Id = "mod1", Name = "Corolla", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "123", Model = model, Owner = owner };
            
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            var createModel = new CreateOrderVM 
            { 
                CarId = "car1", 
                Kilometers = 1000, 
                Jobs = new List<CreateJobVM>() 
            };

            // Act
            var result = (dynamic)await _service.CreateOrderAsync(userId, workshopId, createModel);

            // Assert
            Assert.NotNull(result);
            var orderInDb = _context.Orders.First();
            Assert.Equal(1000, orderInDb.Kilometers);

            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, It.IsAny<string>()), Times.Once);
        }
    }
}
