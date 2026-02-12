using Xunit;
using GarageControl.Core.Services;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using GarageControl.Shared.Enums;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GarageControl.Tests.Services
{
    public class DashboardServiceTests
    {
        private readonly GarageControlDbContext _context;
        private readonly DashboardService _service;

        public DashboardServiceTests()
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GarageControlDbContext(options);
            _service = new DashboardService(_context);
        }

        [Fact]
        public async Task GetDashboardDataAsync_ShouldReturnCorrectStats()
        {
            // Arrange
            var workshopId = "w1";
            var workshop = new Workshop { Id = workshopId, Name = "Workshop", BossId = "boss1", Address = "123 Main St", PhoneNumber = "555-1234" };
            var owner = new Client { Id = "c1", Name = "Client", WorkshopId = workshopId, PhoneNumber = "123-456-7890" };
            var make = new CarMake { Id = "m1", Name = "Toyota" };
            var model = new CarModel { Id = "mod1", Name = "Corolla", CarMake = make };
            var car = new Car { Id = "car1", RegistrationNumber = "123", Model = model, Owner = owner };
            var jobType = new JobType { Id = "jt1", Name = "Oil Change", WorkshopId = workshopId };
            var worker = new Worker { Id = "wkr1", Name = "Worker", WorkshopId = workshopId, UserId = "u1" };
            
            var order1 = new Order { Id = "o1", Car = car };
            var job1 = new Job { Id = "j1", Order = order1, Status = JobStatus.Pending, JobType = jobType, Worker = worker, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
            var job2 = new Job { Id = "j2", Order = order1, Status = JobStatus.InProgress, JobType = jobType, Worker = worker, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };

            _context.Workshops.Add(workshop);
            _context.Clients.Add(owner);
            _context.Cars.Add(car);
            _context.JobTypes.Add(jobType);
            _context.Workers.Add(worker);
            _context.Orders.Add(order1);
            _context.Jobs.AddRange(job1, job2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetDashboardDataAsync(workshopId);

            // Assert
            Assert.Equal(1, result.OrderStats.AllOrders);
            Assert.Equal(1, result.OrderStats.PendingJobs);
            Assert.Equal(1, result.OrderStats.InProgressJobs);
        }
    }
}
