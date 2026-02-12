using Xunit;
using Moq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using GarageControl.Core.ViewModels;

namespace GarageControl.Tests.Services
{
    public class PartServiceTests
    {
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly GarageControlDbContext _context;
        private readonly PartService _service;

        public PartServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _mockActivityLogService = new Mock<IActivityLogService>();
            _mockInventoryService = new Mock<IInventoryService>();

            _service = new PartService(_context, _mockActivityLogService.Object, _mockInventoryService.Object);
        }

        [Fact]
        public async Task CreatePartAsync_ShouldCreatePartAndLog()
        {
            // Arrange
            var userId = "user1";
            var workshopId = "workshop1";
            var model = new CreatePartVM 
            { 
                Name = "Spark Plug", 
                PartNumber = "SP123", 
                Price = 5.50m, 
                Quantity = 10, 
                MinimumQuantity = 2 
            };

            // Act
            var result = await _service.CreatePartAsync(userId, workshopId, model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Spark Plug", result.Name);
            
            var partInDb = await _context.Parts.FindAsync(result.Id);
            Assert.NotNull(partInDb);
            Assert.Equal("SP123", partInDb.PartNumber);

            _mockInventoryService.Verify(x => x.CheckLowStockAsync(workshopId, It.IsAny<Part>()), Times.Once);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, It.Is<string>(s => s.Contains("created part") && s.Contains("Spark Plug"))), Times.Once);
        }

        [Fact]
        public async Task EditPartAsync_ShouldUpdateAndLogChanges()
        {
            // Arrange
            var userId = "user1";
            var workshopId = "workshop1";
            var part = new Part 
            { 
                Id = "part1", 
                Name = "Old Name", 
                PartNumber = "OLD1", 
                Price = 10, 
                Quantity = 5, 
                MinimumQuantity = 1, 
                AvailabilityBalance = 5,
                WorkshopId = workshopId 
            };
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            var model = new UpdatePartVM 
            { 
                Name = "New Name", 
                PartNumber = "NEW1", 
                Price = 12, 
                Quantity = 8, 
                MinimumQuantity = 3 
            };

            // Act
            await _service.EditPartAsync(userId, workshopId, "part1", model);

            // Assert
            var updated = await _context.Parts.FindAsync("part1");
            Assert.Equal("New Name", updated.Name);
            Assert.Equal(12, updated.Price);

            _mockInventoryService.Verify(x => x.RecalculateAvailabilityBalanceAsync(workshopId, "part1"), Times.Once);
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, It.Is<string>(s => s.Contains("updated part") && s.Contains("New Name"))), Times.Once);
        }

        [Fact]
        public async Task DeletePartAsync_ShouldDeleteAndLog()
        {
            // Arrange
            var userId = "user1";
            var workshopId = "workshop1";
            var part = new Part 
            { 
                Id = "part1", 
                Name = "To Delete", 
                PartNumber = "DEL1", 
                Price = 10, 
                Quantity = 5, 
                MinimumQuantity = 0, 
                AvailabilityBalance = 5,
                WorkshopId = workshopId 
            };
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeletePartAsync(userId, workshopId, "part1");

            // Assert
            Assert.Null(await _context.Parts.FindAsync("part1"));
            _mockActivityLogService.Verify(x => x.LogActionAsync(userId, workshopId, It.Is<string>(s => s.Contains("deleted part") && s.Contains("To Delete"))), Times.Once);
        }
    }
}
