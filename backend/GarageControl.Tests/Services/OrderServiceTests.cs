using GarageControl.Core.Models;
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
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Shared;

namespace GarageControl.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly Mock<IJobService> _mockJobService;
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
            _mockJobService = new Mock<IJobService>();

            _service = new OrderService(
                _context, 
                _mockNotificationService.Object, 
                _mockActivityLogService.Object, 
                _mockWorkshopService.Object, 
                _mockInventoryService.Object,
                _mockJobService.Object);
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
            var order = new Order { Id = "o1", Car = car, Kilometers = 1000, IsArchived = false };

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
                Kilometers = 1000
            };

            // Act
            var result = (dynamic)await _service.CreateOrderAsync(userId, workshopId, createModel);

            // Assert
            Assert.NotNull(result);
            var orderInDb = _context.Orders.First();
            Assert.Equal(1000, orderInDb.Kilometers);

            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, "Order", It.IsAny<ActivityLogData>()), Times.Once);
        }
        [Fact]
        public async Task CreateOrderAsync_ShouldSyncCarKilometers()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var owner = new Client { Id = "c1", Name = "Client", WorkshopId = workshopId, PhoneNumber = "123-456-7890" };
            var make = new CarMake { Id = "m1", Name = "Toyota" };
            var model = new CarModel { Id = "mod1", Name = "Corolla", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "123", Model = model, Owner = owner, Kilometers = 500 };
            
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            var createModel = new CreateOrderVM 
            { 
                CarId = "car1", 
                Kilometers = 1000
            };

            // Act
            await _service.CreateOrderAsync(userId, workshopId, createModel);

            // Assert
            var carInDb = await _context.Cars.FindAsync("car1");
            Assert.Equal(1000, carInDb.Kilometers);
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldSyncCarKilometers()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var workshopId = Guid.NewGuid().ToString();
            var ownerId = Guid.NewGuid().ToString();
            var makeId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();
            var carId = Guid.NewGuid().ToString();
            var orderId = Guid.NewGuid().ToString();

            var workshop = new Workshop { Id = workshopId, Name = "Test Workshop", Address = "Test Address", PhoneNumber = "123456", BossId = userId };
            var make = new CarMake { Id = makeId, Name = "Toyota" };
            var model = new CarModel { Id = modelId, Name = "Corolla", CarMakeId = makeId, CarMake = make };
            var owner = new Client { Id = ownerId, Name = "Client", WorkshopId = workshopId, Workshop = workshop, PhoneNumber = "123-456-7890" };
            var car = new Car { Id = carId, RegistrationNumber = "REG_SYNC", ModelId = modelId, Model = model, OwnerId = ownerId, Owner = owner, Kilometers = 1000 };
            var order = new Order { Id = orderId, CarId = carId, Car = car, Kilometers = 1000, IsArchived = false };

            _context.Workshops.Add(workshop);
            _context.CarMakes.Add(make);
            _context.CarModels.Add(model);
            _context.Clients.Add(owner);
            _context.Cars.Add(car);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var updateModel = new UpdateOrderVM 
            { 
                CarId = car.Id, 
                Kilometers = 2000,
                IsArchived = false
            };

            // Act
            var response = await _service.UpdateOrderAsync(userId, orderId, workshopId, updateModel) as MethodResponseVM;
            Assert.NotNull(response);
            Assert.True(response.Success, response.Message);

            // Assert
            var carInDb = await _context.Cars.FindAsync(car.Id);
            Assert.NotNull(carInDb);
            Assert.Equal(2000, carInDb.Kilometers);
            var orderInDb = await _context.Orders.FindAsync(orderId);
            Assert.NotNull(orderInDb);
            Assert.Equal(2000, orderInDb.Kilometers);
        }
    }
}
