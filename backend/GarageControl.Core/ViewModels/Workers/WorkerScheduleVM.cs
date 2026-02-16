namespace GarageControl.Core.ViewModels.Workers
{
    public class WorkerScheduleVM
    {
        public string? Id { get; set; }
        public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, etc.
        public string StartTime { get; set; } = "09:00"; // HH:mm format
        public string EndTime { get; set; } = "17:00"; // HH:mm format
    }
}
