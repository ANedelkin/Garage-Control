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
using System.Linq;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Clients;
using Microsoft.EntityFrameworkCore;
using System;

namespace GarageControl.Tests.Services
{
    public class ClientServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly IRepository _repo;
        private readonly Mock<IWorkshopService> _mockWorkshopService;
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly ClientService _service;

        public ClientServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _repo = new Repository(_context);
            _mockWorkshopService = new Mock<IWorkshopService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            _service = new ClientService(_repo, _mockWorkshopService.Object, _mockActivityLogService.Object);
        }

        [Fact]
        public async Task All_ShouldReturnClientsForWorkshop()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            
            _context.Clients.AddRange(new List<Client>
            {
                new Client { Id = "c1", Name = "Client 1", WorkshopId = workshopId, Email = "c1@test.com", PhoneNumber = "123" },
                new Client { Id = "c2", Name = "Client 2", WorkshopId = "w2", Email = "c2@test.com", PhoneNumber = "456" }
            });
            await _context.SaveChangesAsync();

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);

            // Act
            var result = await _service.All(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Client 1", result.First().Name);
        }

        [Fact]
        public async Task Create_ShouldCreateAndLog()
        {
            // Arrange
            var userId = "u1";
            var workshopId = "w1";
            var model = new ClientVM { Name = "New Client", Email = "new@test.com", PhoneNumber = "123" };

            _mockWorkshopService.Setup(x => x.GetWorkshopId(userId)).ReturnsAsync(workshopId);

            // Act
            await _service.Create(model, userId);

            // Assert
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Name == "New Client");
            Assert.NotNull(client);
            Assert.Equal(workshopId, client.WorkshopId);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, "Client", It.Is<ActivityLogData>(d => d.Action == "created" && d.EntityName == "New Client")), Times.Once);
        }
    }
}
