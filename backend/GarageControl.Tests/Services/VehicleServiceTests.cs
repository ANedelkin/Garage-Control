using GarageControl.Core.Models;
using Xunit;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;
using Moq;
using GarageControl.Core.Contracts;

namespace GarageControl.Tests.Services
{
    public class VehicleServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly IRepository _repo;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly VehicleService _service;

        public VehicleServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _repo = new Repository(_context);
            _mockWorkshopService = new Mock<IWorkshopService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            _service = new VehicleService(_repo, _mockWorkshopService.Object, _mockActivityLogService.Object);
        }

        [Fact]
        public async Task Create_ShouldAddCarAndLog()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var clientId = "c1";
            var modelId = "m1";
            var model = new VehicleVM { OwnerId = clientId, ModelId = modelId, RegistrationNumber = "REG1", Kilometers = 1000 };
            
            var client = new Client { Id = clientId, WorkshopId = workshopId, Name = "Client", PhoneNumber = "123-456-7890" };
            _context.Clients.Add(client);
            
            var make = new CarMake { Id = "mk1", Name = "Toyota" };
            var carModel = new CarModel { Id = modelId, Name = "Corolla", CarMake = make };
            _context.CarModels.Add(carModel);
            await _context.SaveChangesAsync();
            
            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);

            // Act
            await _service.Create(model, userId);

            // Assert
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.RegistrationNumber == "REG1");
            Assert.NotNull(car);
            Assert.Equal(clientId, car.OwnerId);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, "Vehicle", It.Is<ActivityLogData>(d => d.Action == "added" && d.EntityName.Contains("REG1"))), Times.Once);
        }

        [Fact]
        public async Task All_ShouldReturnVehiclesForWorkshop()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            
            var client = new Client { Id = "c1", Name = "C1", WorkshopId = workshopId, PhoneNumber = "123" };
            var make = new CarMake { Id = "m1", Name = "Make1" };
            var carModel = new CarModel { Id = "mod1", Name = "Mod1", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "R1", Owner = client, Model = carModel };
            
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);

            // Act
            var result = await _service.All(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("R1", result.First().RegistrationNumber);
        }
    }
}
