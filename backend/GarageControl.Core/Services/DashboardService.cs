using GarageControl.Core.ViewModels.Dashboard;
using GarageControl.Infrastructure.Data;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string workshopId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly GarageControlDbContext _context;

        public DashboardService(GarageControlDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string workshopId)
        {

            var dashboard = new DashboardViewModel
            {
                OrderStats = await GetOrderStatsAsync(workshopId),
                JobsCompletedByDay = await GetJobsCompletedByDayAsync(workshopId),
                LowStockParts = await GetLowStockPartsAsync(workshopId),
                JobTypeDistribution = await GetJobTypeDistributionAsync(workshopId),
                WorkerPerformance = await GetWorkerPerformanceAsync(workshopId),
            };

            return dashboard;
        }

        private async Task<OrderStatsViewModel> GetOrderStatsAsync(string workshopId)
        {
            var allOrders = await _context.Orders
                .Where(o => o.Car.Owner.WorkshopId == workshopId)
                .CountAsync();

            var pendingJobs = await _context.Jobs
                .Where(j => j.Order.Car.Owner.WorkshopId == workshopId && j.Status == JobStatus.Pending)
                .CountAsync();

            var inProgressJobs = await _context.Jobs
                .Where(j => j.Order.Car.Owner.WorkshopId == workshopId && j.Status == JobStatus.InProgress)
                .CountAsync();

            return new OrderStatsViewModel
            {
                AllOrders = allOrders,
                PendingJobs = pendingJobs,
                InProgressJobs = inProgressJobs
            };
        }

        private async Task<List<JobsCompletedByDayViewModel>> GetJobsCompletedByDayAsync(string workshopId)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).Date;

            var completedJobs = await _context.Jobs
                .Where(j => j.Order.Car.Owner.WorkshopId == workshopId 
                    && j.Status == JobStatus.Done 
                    && j.EndTime >= thirtyDaysAgo)
                .Select(j => new
                {
                    // If job is done but scheduled end time is in future, count it as done today
                    Date = (j.EndTime > DateTime.UtcNow ? DateTime.UtcNow : j.EndTime).Date,
                    JobTypeName = j.JobType.Name
                })
                .ToListAsync();

            var groupedByDay = completedJobs
                .GroupBy(j => j.Date)
                .Select(g => new JobsCompletedByDayViewModel
                {
                    Date = g.Key,
                    JobTypesCounts = g.GroupBy(x => x.JobTypeName)
                                      .ToDictionary(jt => jt.Key, jt => jt.Count())
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Fill in missing days with empty data
            var allDays = new List<JobsCompletedByDayViewModel>();
            var today = DateTime.UtcNow.Date;
            for (int i = 0; i < 30; i++)
            {
                var date = today.AddDays(-29 + i);
                var existing = groupedByDay.FirstOrDefault(x => x.Date == date);
                allDays.Add(existing ?? new JobsCompletedByDayViewModel { Date = date });
            }

            return allDays;
        }

        private async Task<List<LowStockPartViewModel>> GetLowStockPartsAsync(string workshopId)
        {
            return await _context.Parts
                .Where(p => p.WorkshopId == workshopId && p.AvailabilityBalance < p.MinimumQuantity)
                .Select(p => new LowStockPartViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    CurrentQuantity = p.AvailabilityBalance, // Show available balance in the tile
                    MinimumQuantity = p.MinimumQuantity
                })
                .OrderBy(p => p.CurrentQuantity)
                .ToListAsync();
        }

        private async Task<List<JobTypeDistributionViewModel>> GetJobTypeDistributionAsync(string workshopId)
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

            var distribution = await _context.Jobs
                .Where(j => j.Order.Car.Owner.WorkshopId == workshopId && j.Status == JobStatus.Done && j.EndTime >= oneMonthAgo)
                .GroupBy(j => j.JobType.Name)
                .Select(g => new
                {
                    JobTypeName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return distribution.Select(x => new JobTypeDistributionViewModel
            {
                JobTypeName = x.JobTypeName,
                Count = x.Count,
            }).ToList();
        }

        private async Task<List<WorkerPerformanceViewModel>> GetWorkerPerformanceAsync(string workshopId)
        {
            var workers = await _context.Workers
                .Where(w => w.WorkshopId == workshopId)
                .Select(w => new
                {
                    w.Id,
                    w.Name,
                    Jobs = w.Jobs.Where(j => j.Order.Car.Owner.WorkshopId == workshopId && j.Status == JobStatus.Done)
                                 .Select(j => new
                                 {
                                     JobTypeName = j.JobType.Name,
                                     HoursWorked = (j.EndTime - j.StartTime).TotalHours
                                 })
                                 .ToList()
                })
                .ToListAsync();

            return workers.Select(w => new WorkerPerformanceViewModel
            {
                WorkerId = w.Id,
                WorkerName = w.Name,
                JobTypesCounts = w.Jobs.GroupBy(j => j.JobTypeName)
                                       .ToDictionary(g => g.Key, g => g.Count()),
                TotalHoursWorked = Math.Round(w.Jobs.Sum(j => j.HoursWorked), 2)
            }).ToList();
        }
    }
}
