using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;
using GarageControl.Core.ViewModels.Auth;

namespace GarageControl.Core.ViewModels.Workers
{
    public class WorkerVM
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(WorkerConstants.nameMaxLength, ErrorMessage = "Name cannot exceed {1} characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Username is required.")]
        [MinLength(AuthConstants.usernameMinLength, ErrorMessage = "Username must be at least {1} characters.")]
        [MaxLength(AuthConstants.usernameMaxLength, ErrorMessage = "Username cannot exceed {1} characters.")]
        public string Username { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(GenericConstants.emailMaxLength, ErrorMessage = "Email cannot exceed {1} characters.")]
        public string? Email { get; set; }

        public string? Password { get; set; }
        public DateTime HiredOn { get; set; } = DateTime.Today;

        public List<AccessVM> Accesses { get; set; } = new List<AccessVM>();
        public List<string> JobTypeIds { get; set; } = new List<string>(); // IDs of JobTypes this worker can perform
        public List<string> JobTypeNames { get; set; } = new List<string>();
        public List<WorkerScheduleVM> Schedules { get; set; } = new List<WorkerScheduleVM>();
        public List<WorkerLeaveVM> Leaves { get; set; } = new List<WorkerLeaveVM>();
    }
}
