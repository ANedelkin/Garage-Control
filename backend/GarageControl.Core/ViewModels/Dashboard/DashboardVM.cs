using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.ViewModels.Workers;

namespace GarageControl.Core.ViewModels.Dashboard
{
    public class DashboardVM
    {
        public OrderStatsVM OrderStats { get; set; } = null!;
        public List<JobsCompletedByDayVM> JobsCompletedByDay { get; set; } = new();
        public List<LowStockPartVM> LowStockParts { get; set; } = new();
        public List<JobTypeDistributionVM> JobTypeDistribution { get; set; } = new();
        public List<WorkerPerformanceVM> WorkerPerformance { get; set; } = new();
    }
}
