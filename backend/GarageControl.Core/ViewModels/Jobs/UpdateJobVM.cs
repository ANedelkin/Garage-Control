using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;
using GarageControl.Shared.Enums;

namespace GarageControl.Core.ViewModels.Jobs
{
    public class UpdateJobVM
    {
        public string? Id { get; set; } // If null, it's a new job
        [Required(ErrorMessage = "Job type required")]
        public string JobTypeId { get; set; } = null!;
        [StringLength(JobConstants.descriptionMaxLength)]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Mechanic required")]
        public string WorkerId { get; set; } = null!;
        public JobStatus Status { get; set; }
        [Range(0, double.MaxValue)]
        public decimal LaborCost { get; set; }
        [Required(ErrorMessage = "Time slot required")]
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<CreateJobPartVM> Parts { get; set; } = new List<CreateJobPartVM>();
    }
}
