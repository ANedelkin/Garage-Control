using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Dashboard;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.ViewModels.Workers;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly GarageControlDbContext _context;

        public DashboardService(GarageControlDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardVM> GetDashboardDataAsync(string workshopId)
        {
            var now = DateTime.UtcNow;

            var dashboard = new DashboardVM
            {
                OrderStats = await GetOrderStatsAsync(workshopId),
                JobsCompletedByDay = await GetJobsCompletedByDayAsync(workshopId, now),
                LowStockParts = await GetLowStockPartsAsync(workshopId),
                JobTypeDistribution = await GetJobTypeDistributionAsync(workshopId, now),
                WorkerPerformance = await GetWorkerPerformanceAsync(workshopId)
            };

            return dashboard;
        }

        private IQueryable<Job> JobsForWorkshop(string workshopId)
        {
            return _context.Jobs
                .AsNoTracking()
                .Where(j => j.Order.Car.Owner.WorkshopId == workshopId);
        }

        private async Task<OrderStatsVM> GetOrderStatsAsync(string workshopId)
        {
            var stats = await JobsForWorkshop(workshopId)
                .GroupBy(_ => 1)
                .Select(g => new OrderStatsVM
                {
                    PendingJobs = g.Count(j => j.Status == JobStatus.Pending),
                    InProgressJobs = g.Count(j => j.Status == JobStatus.InProgress),
                })
                .Select(s => new OrderStatsVM
                {
                    PendingJobs = s.PendingJobs,
                    InProgressJobs = s.InProgressJobs,
                    AllOrders = s.PendingJobs + s.InProgressJobs,
                })
                .FirstOrDefaultAsync();

            return stats ?? new OrderStatsVM();
        }

        private async Task<List<JobsCompletedByDayVM>> GetJobsCompletedByDayAsync(string workshopId, DateTime now)
        {
            const int daysPeriod = 30;
            var thirtyDaysAgo = now.AddDays(-daysPeriod).Date;

            var grouped = await JobsForWorkshop(workshopId)
                .Where(j => j.Status == JobStatus.Done && j.EndTime >= thirtyDaysAgo)
                .Select(j => new
                {
                    Date = (j.EndTime > now ? now : j.EndTime).Date,
                    JobTypeName = j.JobType.Name
                })
                .GroupBy(j => new { j.Date, j.JobTypeName })
                .Select(g => new
                {
                    g.Key.Date,
                    g.Key.JobTypeName,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = grouped
                .GroupBy(x => x.Date)
                .Select(g => new JobsCompletedByDayVM
                {
                    Date = g.Key,
                    JobTypesCounts = g.ToDictionary(x => x.JobTypeName, x => x.Count)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // fill missing days
            var full = new List<JobsCompletedByDayVM>();
            var today = now.Date;

            for (int i = 1; i <= daysPeriod; i++)
            {
                var date = today.AddDays(-daysPeriod + i);
                full.Add(result.FirstOrDefault(x => x.Date == date)
                    ?? new JobsCompletedByDayVM { Date = date });
            }

            return full;
        }

        private async Task<List<LowStockPartVM>> GetLowStockPartsAsync(string workshopId)
        {
            return await _context.Parts
                .AsNoTracking()
                .Where(p => p.WorkshopId == workshopId && p.AvailabilityBalance < p.MinimumQuantity)
                .Select(p => new LowStockPartVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    CurrentQuantity = p.AvailabilityBalance,
                    MinimumQuantity = p.MinimumQuantity
                })
                .OrderBy(p => p.CurrentQuantity)
                .ToListAsync();
        }

        private async Task<List<JobTypeDistributionVM>> GetJobTypeDistributionAsync(string workshopId, DateTime now)
        {
            const int daysPeriod = 30;
            var oneMonthAgo = now.AddDays(-daysPeriod);

            return await JobsForWorkshop(workshopId)
                .Where(j => j.Status == JobStatus.Done && j.EndTime >= oneMonthAgo)
                .GroupBy(j => j.JobType.Name)
                .Select(g => new JobTypeDistributionVM
                {
                    JobTypeName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        private async Task<List<WorkerPerformanceVM>> GetWorkerPerformanceAsync(string workshopId)
        {
            var jobs = await JobsForWorkshop(workshopId)
                .Where(j => j.Status == JobStatus.Done)
                .Select(j => new
                {
                    j.WorkerId,
                    WorkerName = j.Worker.Name,
                    JobTypeName = j.JobType.Name,
                    HoursWorked = (j.EndTime - j.StartTime).TotalHours
                })
                .ToListAsync();

            return jobs
                .GroupBy(j => new { j.WorkerId, j.WorkerName })
                .Select(g => new WorkerPerformanceVM
                {
                    WorkerId = g.Key.WorkerId,
                    WorkerName = g.Key.WorkerName,
                    JobTypesCounts = g.GroupBy(x => x.JobTypeName)
                        .ToDictionary(x => x.Key, x => x.Count()),
                    TotalHoursWorked = Math.Round(g.Sum(x => x.HoursWorked), 2)
                })
                .ToList();
        }
    }
}
