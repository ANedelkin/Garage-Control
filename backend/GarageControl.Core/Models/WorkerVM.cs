using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.Models
{
    public class WorkerVM
    {
        public string? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public DateTime HiredOn { get; set; } = DateTime.Today;

        public List<RoleVM> Roles { get; set; } = new List<RoleVM>();
        public List<string> JobTypeIds { get; set; } = new List<string>(); // IDs of JobTypes this worker can perform
        public List<WorkerScheduleVM> Schedules { get; set; } = new List<WorkerScheduleVM>();
        public List<WorkerLeaveVM> Leaves { get; set; } = new List<WorkerLeaveVM>();
    }

    public class RoleVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsSelected { get; set; }
    }

    public class WorkerScheduleVM
    {
        public string? Id { get; set; }
        public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, etc.
        public string StartTime { get; set; } = "09:00"; // HH:mm format
        public string EndTime { get; set; } = "17:00"; // HH:mm format
    }

    public class WorkerLeaveVM
    {
        public string? Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
