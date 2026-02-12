using Xunit;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using GarageControl.Core.ViewModels;

namespace GarageControl.Tests.Services
{
    public class WorkshopServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly IRepository _repo;
        private readonly WorkshopService _service;

        public WorkshopServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _repo = new Repository(_context);
            _service = new WorkshopService(_repo);
        }

        [Fact]
        public async Task CreateWorkshop_ShouldAddWorkshop()
        {
            // Arrange
            var userId = "u1";
            var model = new WorkshopVM 
            { 
                Name = "G1", 
                Address = "A1", 
                PhoneNumber = "123", 
                Email = "g1@test.com", 
                RegistrationNumber = "R1" 
            };

            // Act
            await _service.CreateWorkshop(userId, model);

            // Assert
            var workshop = await _context.Workshops.FirstOrDefaultAsync(w => w.BossId == userId);
            Assert.NotNull(workshop);
            Assert.Equal("G1", workshop.Name);
        }

        [Fact]
        public async Task GetWorkshopId_ShouldReturnIdForOwner()
        {
            // Arrange
            var userId = "u1";
            var workshop = new Workshop { Id = "w1", BossId = userId, Name = "W", Address = "A", PhoneNumber = "123" };
            _context.Workshops.Add(workshop);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetWorkshopId(userId);

            // Assert
            Assert.Equal("w1", result);
        }

        [Fact]
        public async Task GetWorkshopId_ShouldReturnIdForWorker()
        {
            // Arrange
            var userId = "u1";
            var workshop = new Workshop { Id = "w2", BossId = "boss", Name = "W", Address = "A", PhoneNumber = "123" };
            var worker = new Worker { Id = "wkr1", UserId = userId, WorkshopId = "w2", Name = "Worker" };
            
            _context.Workshops.Add(workshop);
            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetWorkshopId(userId);

            // Assert
            Assert.Equal("w2", result);
        }
    }
}
