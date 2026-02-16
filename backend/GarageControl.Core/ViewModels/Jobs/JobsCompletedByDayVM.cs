namespace GarageControl.Core.ViewModels.Jobs
{
    public class JobsCompletedByDayVM
    {
        public DateTime Date { get; set; }
        public Dictionary<string, int> JobTypesCounts { get; set; } = new();
    }
}
