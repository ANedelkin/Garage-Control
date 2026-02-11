namespace GarageControl.Core.ViewModels
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
