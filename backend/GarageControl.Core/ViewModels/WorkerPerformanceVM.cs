namespace GarageControl.Core.ViewModels
{
    public class WorkerPerformanceVM
    {
        public string WorkerId { get; set; } = null!;
        public string WorkerName { get; set; } = null!;
        public Dictionary<string, int> JobTypesCounts { get; set; } = new();
        public double TotalHoursWorked { get; set; }
    }
}
