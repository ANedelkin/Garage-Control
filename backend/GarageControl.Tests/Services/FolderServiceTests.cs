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
using GarageControl.Core.ViewModels.Parts;

namespace GarageControl.Tests.Services
{
    public class FolderServiceTests
    {
        private readonly Mock<IActivityLogService> _mockActivityLogService;
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly Mock<IDeficitService> _mockDeficitService;
        private readonly GarageControlDbContext _context;
        private readonly FolderService _service;

        public FolderServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _mockActivityLogService = new Mock<IActivityLogService>();
            _mockInventoryService = new Mock<IInventoryService>();
            _mockDeficitService = new Mock<IDeficitService>();

            _service = new FolderService(_context, _mockActivityLogService.Object, _mockInventoryService.Object, _mockDeficitService.Object);
        }

        [Fact]
        public async Task CreateFolderAsync_ShouldCreateFolderAndLogActivity()
        {
            // Arrange
            var userId = "user1";
            var workshopId = "workshop1";
            var model = new CreateFolderVM { Name = "NewFolder", ParentId = null };

            // Act
            var result = await _service.CreateFolderAsync(userId, workshopId, model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewFolder", result.Name);
            Assert.Null(result.ParentId);

            var folderInDb = await _context.PartsFolders.FirstOrDefaultAsync(f => f.Id == result.Id);
            Assert.NotNull(folderInDb);
            Assert.Equal("NewFolder", folderInDb.Name);

            // Verify logging
            _mockActivityLogService.Verify(x => x.LogActionAsync(
                userId, 
                workshopId, 
                It.Is<string>(s => s.Contains("created group of parts") && s.Contains("NewFolder"))), Times.Once);
        }

        [Fact]
        public async Task GetFolderContentAsync_ShouldReturnCorrectContent()
        {
            // Arrange
            var workshopId = "workshop1";
            var parentFolder = new PartsFolder { Id = "parent", Name = "Parent", WorkshopId = workshopId };
            var childFolder = new PartsFolder { Id = "child", Name = "Child", ParentId = "parent", WorkshopId = workshopId };
            var part = new Part 
            { 
                Id = "part1", 
                Name = "Part1", 
                PartNumber = "PN1",
                Price = 10,
                Quantity = 5,
                MinimumQuantity = 1,
                AvailabilityBalance = 5,
                ParentId = "parent", 
                WorkshopId = workshopId 
            };

            _context.PartsFolders.AddRange(parentFolder, childFolder);
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            _mockInventoryService.Setup(x => x.GetPartsToSendAsync(workshopId, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, int> { { part.Id, 2 } });

            // Act
            var result = await _service.GetFolderContentAsync(workshopId, "parent");

            // Assert
            Assert.Equal("parent", result.CurrentFolderId);
            Assert.Equal("Parent", result.CurrentFolderName);
            Assert.Single(result.SubFolders);
            Assert.Equal("child", result.SubFolders.First().Id);
            Assert.Single(result.Parts);
            Assert.Equal("part1", result.Parts[0].Id);
            Assert.Equal(2.0, result.Parts[0].PartsToSend);
        }

        [Fact]
        public async Task DeleteFolderAsync_ShouldDeleteRecursivelyAndLog()
        {
            // Arrange
            var userId = "user1";
            var workshopId = "workshop1";
            var parentFolder = new PartsFolder { Id = "parent", Name = "Parent", WorkshopId = workshopId };
            var childFolder = new PartsFolder { Id = "child", Name = "Child", ParentId = "parent", WorkshopId = workshopId };
            var part = new Part 
            { 
                Id = "part1", 
                Name = "Part1", 
                PartNumber = "PN1",
                Price = 10,
                Quantity = 5,
                MinimumQuantity = 1,
                AvailabilityBalance = 5,
                ParentId = "child", 
                WorkshopId = workshopId 
            };

            _context.PartsFolders.AddRange(parentFolder, childFolder);
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteFolderAsync(userId, workshopId, "parent");

            // Assert
            Assert.Null(await _context.PartsFolders.FindAsync("parent"));
            Assert.Null(await _context.PartsFolders.FindAsync("child"));
            Assert.Null(await _context.Parts.FindAsync("part1"));

            _mockActivityLogService.Verify(x => x.LogActionAsync(
                userId,
                workshopId,
                It.Is<string>(s => s.Contains("deleted group of parts"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task MoveFolderAsync_ShouldMoveFolderAndLog()
        {
            // Arrange
            var userId = "user1";
            var workshopId = "workshop1";
            var folder = new PartsFolder { Id = "folder", Name = "Folder", ParentId = "oldParent", WorkshopId = workshopId };
            var oldParent = new PartsFolder { Id = "oldParent", Name = "OldParent", WorkshopId = workshopId };
            var newParent = new PartsFolder { Id = "newParent", Name = "NewParent", WorkshopId = workshopId };

            _context.PartsFolders.AddRange(folder, oldParent, newParent);
            await _context.SaveChangesAsync();

            // Act
            await _service.MoveFolderAsync(userId, workshopId, "folder", "newParent");

            // Assert
            var movedFolder = await _context.PartsFolders.FindAsync("folder");
            Assert.Equal("newParent", movedFolder.ParentId);

             _mockActivityLogService.Verify(x => x.LogActionAsync(
                userId, 
                workshopId,
                It.Is<string>(s => 
                    s.Contains("moved group of parts") && 
                    s.Contains("Folder") && 
                    s.Contains("OldParent") && 
                    s.Contains("NewParent"))), Times.Once);
        }
    }
}
