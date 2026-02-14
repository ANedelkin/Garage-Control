using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageControl.Core.Services;
using GarageControl.Core.ViewModels;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GarageControl.Tests.Services
{
    public class DashboardServiceTests
    {
        // ---------------------- Helper to create DB and service ----------------------
        private (GarageControlDbContext Context, DashboardService Service) CreateService(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<GarageControlDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new GarageControlDbContext(options);
            var service = new DashboardService(context);
            return (context, service);
        }

        // ---------------------- Helpers to seed entities ----------------------
        private async Task<Client> CreateClientAsync(GarageControlDbContext context, string workshopId)
        {
            var client = new Client { Id = Guid.NewGuid().ToString(), Name = "Client", PhoneNumber = "123", WorkshopId = workshopId };
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            return client;
        }

        private async Task<Worker> CreateWorkerAsync(GarageControlDbContext context, string id, string userId, string workshopId)
        {
            var worker = new Worker { Id = id, UserId = userId, Name = "Worker", WorkshopId = workshopId };
            context.Workers.Add(worker);
            await context.SaveChangesAsync();
            return worker;
        }

        private async Task<JobType> CreateJobTypeAsync(GarageControlDbContext context, string name, string workshopId)
        {
            var jt = new JobType { Id = Guid.NewGuid().ToString(), Name = name, WorkshopId = workshopId };
            context.JobTypes.Add(jt);
            await context.SaveChangesAsync();
            return jt;
        }

        private async Task<Order> CreateOrderAsync(GarageControlDbContext context, string workshopId)
        {
            var client = await CreateClientAsync(context, workshopId);
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                Car = new Car { Id = Guid.NewGuid().ToString(), ModelId = "M", RegistrationNumber = "R", Owner = client }
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            return order;
        }

        private async Task<Job> CreateJobAsync(GarageControlDbContext context, Worker worker, Order order, JobType jobType, JobStatus status, DateTime start, DateTime end)
        {
            var job = new Job
            {
                Id = Guid.NewGuid().ToString(),
                Worker = worker,
                Order = order,
                JobType = jobType,
                Status = status,
                StartTime = start,
                EndTime = end
            };
            context.Jobs.Add(job);
            await context.SaveChangesAsync();
            return job;
        }

        // ---------------------- TESTS ----------------------

        [Fact]
        public async Task OrderStats_CountsJobsCorrectly_IgnoresOtherWorkshops()
        {
            var (context, service) = CreateService();
            var workshopId = "W1";

            var order1 = await CreateOrderAsync(context, workshopId);
            var order2 = await CreateOrderAsync(context, "Other");

            var worker1 = await CreateWorkerAsync(context, "W1", "U1", workshopId);
            var worker2 = await CreateWorkerAsync(context, "W2", "U2", workshopId);
            var worker3 = await CreateWorkerAsync(context, "W3", "U3", "Other");

            var jt = await CreateJobTypeAsync(context, "Repair", workshopId);

            await CreateJobAsync(context, worker1, order1, jt, JobStatus.Pending, DateTime.UtcNow, DateTime.UtcNow);
            await CreateJobAsync(context, worker2, order1, jt, JobStatus.InProgress, DateTime.UtcNow, DateTime.UtcNow);
            await CreateJobAsync(context, worker3, order2, jt, JobStatus.Pending, DateTime.UtcNow, DateTime.UtcNow);

            var dashboard = await service.GetDashboardDataAsync(workshopId);

            Assert.Equal(1, dashboard.OrderStats.PendingJobs);
            Assert.Equal(1, dashboard.OrderStats.InProgressJobs);
            Assert.Equal(2, dashboard.OrderStats.AllOrders);
        }

        [Fact]
        public async Task LowStockParts_FiltersCorrectly()
        {
            var (context, service) = CreateService();
            var workshopId = "W1";

            context.Parts.AddRange(
                new Part { Id = "P1", Name = "Brake Pads", PartNumber = "BP1", WorkshopId = workshopId, AvailabilityBalance = 2, MinimumQuantity = 5 },
                new Part { Id = "P2", Name = "Oil Filter", PartNumber = "OF1", WorkshopId = workshopId, AvailabilityBalance = 5, MinimumQuantity = 5 },
                new Part { Id = "P3", Name = "Air Filter", PartNumber = "AF1", WorkshopId = "Other", AvailabilityBalance = 1, MinimumQuantity = 5 }
            );
            await context.SaveChangesAsync();

            var dashboard = await service.GetDashboardDataAsync(workshopId);

            Assert.Single(dashboard.LowStockParts);
            Assert.Equal("P1", dashboard.LowStockParts[0].Id);
        }

        [Fact]
        public async Task JobsCompletedByDay_IncludesBoundaryJobs_AndIgnoresOutside()
        {
            var (context, service) = CreateService();
            var workshopId = "W1";
            var now = DateTime.UtcNow;

            var order = await CreateOrderAsync(context, workshopId);
            var worker = await CreateWorkerAsync(context, "W1", "U1", workshopId);

            var jtBoundary = await CreateJobTypeAsync(context, "BoundaryJob", workshopId);
            var jtOutside = await CreateJobTypeAsync(context, "OutsideJob", "Other");

            await CreateJobAsync(context, worker, order, jtBoundary, JobStatus.Done, now.AddDays(-1), now.AddDays(-1));
            await CreateJobAsync(context, worker, order, jtOutside, JobStatus.Done, now.AddDays(-31), now.AddDays(-31));

            var dashboard = await service.GetDashboardDataAsync(workshopId);

            Assert.Equal(30, dashboard.JobsCompletedByDay.Count);
            Assert.Contains(dashboard.JobsCompletedByDay, x => x.JobTypesCounts.ContainsKey("BoundaryJob"));
            Assert.DoesNotContain(dashboard.JobsCompletedByDay, x => x.JobTypesCounts.ContainsKey("OutsideJob"));
        }

        [Fact]
        public async Task JobTypeDistribution_IncludesOnlyBoundaryJobs()
        {
            var (context, service) = CreateService();
            var workshopId = "W1";
            var now = DateTime.UtcNow;

            var order = await CreateOrderAsync(context, workshopId);
            var worker = await CreateWorkerAsync(context, "W1", "U1", workshopId);

            var jtRecent = await CreateJobTypeAsync(context, "RecentRepair", workshopId);
            var jtOld = await CreateJobTypeAsync(context, "OldRepair", "Other");

            await CreateJobAsync(context, worker, order, jtRecent, JobStatus.Done, now.AddDays(-1), now.AddDays(-1));
            await CreateJobAsync(context, worker, order, jtOld, JobStatus.Done, now.AddDays(-31), now.AddDays(-31));

            var dashboard = await service.GetDashboardDataAsync(workshopId);

            Assert.Contains(dashboard.JobTypeDistribution, x => x.JobTypeName == "RecentRepair");
            Assert.DoesNotContain(dashboard.JobTypeDistribution, x => x.JobTypeName == "OldRepair");
        }

        [Fact]
        public async Task WorkerPerformance_AggregatesCorrectly()
        {
            var (context, service) = CreateService();
            var workshopId = "W1";
            var now = DateTime.UtcNow;

            var order = await CreateOrderAsync(context, workshopId);
            var worker1 = await CreateWorkerAsync(context, "W1", "U1", workshopId);
            var worker2 = await CreateWorkerAsync(context, "W2", "U2", workshopId);

            var jtRepair = await CreateJobTypeAsync(context, "Repair", workshopId);
            var jtMaintenance = await CreateJobTypeAsync(context, "Maintenance", workshopId);

            await CreateJobAsync(context, worker1, order, jtRepair, JobStatus.Done, now.AddHours(-3), now.AddHours(-1));
            await CreateJobAsync(context, worker1, order, jtRepair, JobStatus.Done, now.AddHours(-2), now.AddHours(-1));
            await CreateJobAsync(context, worker2, order, jtMaintenance, JobStatus.Done, now.AddHours(-1), now);

            var dashboard = await service.GetDashboardDataAsync(workshopId);

            var firstWorker = dashboard.WorkerPerformance.First(x => x.WorkerName == "Worker");
            Assert.Equal(2, firstWorker.JobTypesCounts["Repair"]);
            Assert.True(firstWorker.TotalHoursWorked > 0);
        }

        [Fact]
        public async Task Dashboard_ReturnsAllSections()
        {
            var (context, service) = CreateService();
            var workshopId = "W1";
            var now = DateTime.UtcNow;

            var order = await CreateOrderAsync(context, workshopId);
            var worker = await CreateWorkerAsync(context, "W1", "U1", workshopId);
            var jt = await CreateJobTypeAsync(context, "Repair", workshopId);

            await CreateJobAsync(context, worker, order, jt, JobStatus.Done, now.AddHours(-2), now);

            context.Parts.Add(new Part { Id = "P1", Name = "Brake Pads", PartNumber = "BP1", WorkshopId = workshopId, AvailabilityBalance = 2, MinimumQuantity = 5 });
            await context.SaveChangesAsync();

            var dashboard = await service.GetDashboardDataAsync(workshopId);

            Assert.NotNull(dashboard.OrderStats);
            Assert.Single(dashboard.LowStockParts);
            Assert.Equal(30, dashboard.JobsCompletedByDay.Count);
            Assert.NotEmpty(dashboard.JobTypeDistribution);
            Assert.NotEmpty(dashboard.WorkerPerformance);
        }
    }
}
