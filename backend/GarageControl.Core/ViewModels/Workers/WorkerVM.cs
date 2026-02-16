using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;
using GarageControl.Core.ViewModels.Auth;

namespace GarageControl.Core.ViewModels.Workers
{
    public class WorkerVM
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(WorkerConstants.nameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(GenericConstants.emailMaxLength)]
        public string Email { get; set; } = null!;

        public string? Password { get; set; }
        public DateTime HiredOn { get; set; } = DateTime.Today;

        public List<AccessVM> Accesses { get; set; } = new List<AccessVM>();
        public List<string> JobTypeIds { get; set; } = new List<string>(); // IDs of JobTypes this worker can perform
        public List<WorkerScheduleVM> Schedules { get; set; } = new List<WorkerScheduleVM>();
        public List<WorkerLeaveVM> Leaves { get; set; } = new List<WorkerLeaveVM>();
    }
}
