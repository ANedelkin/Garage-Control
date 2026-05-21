namespace GarageControl.Core.ViewModels.Orders
{
    public class JobStatsVM
    {
        public int ActiveJobs { get; set; }
        public int PendingJobs { get; set; }
        public int InProgressJobs { get; set; }
    }
}
