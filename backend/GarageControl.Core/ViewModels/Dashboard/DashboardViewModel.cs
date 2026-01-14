namespace GarageControl.Core.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public OrderStatsViewModel OrderStats { get; set; } = null!;
        public List<JobsCompletedByDayViewModel> JobsCompletedByDay { get; set; } = new();
        public List<LowStockPartViewModel> LowStockParts { get; set; } = new();
        public List<JobTypeDistributionViewModel> JobTypeDistribution { get; set; } = new();
        public List<WorkerPerformanceViewModel> WorkerPerformance { get; set; } = new();
    }

    public class OrderStatsViewModel
    {
        public int AllOrders { get; set; }
        public int PendingJobs { get; set; }
        public int InProgressJobs { get; set; }
    }

    public class JobsCompletedByDayViewModel
    {
        public DateTime Date { get; set; }
        public Dictionary<string, int> JobTypesCounts { get; set; } = new();
    }

    public class LowStockPartViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int CurrentQuantity { get; set; }
        public int MinimumQuantity { get; set; }
    }

    public class JobTypeDistributionViewModel
    {
        public string JobTypeName { get; set; } = null!;
        public int Count { get; set; }
    }

    public class WorkerPerformanceViewModel
    {
        public string WorkerId { get; set; } = null!;
        public string WorkerName { get; set; } = null!;
        public Dictionary<string, int> JobTypesCounts { get; set; } = new();
        public double TotalHoursWorked { get; set; }
    }
}
