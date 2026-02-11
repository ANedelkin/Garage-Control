using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;
using GarageControl.Shared.Enums;

namespace GarageControl.Core.ViewModels
{
    public class CreateJobVM
    {
        [Required]
        public string JobTypeId { get; set; } = null!;
        [StringLength(JobConstants.descriptionMaxLength)]
        public string? Description { get; set; }
        [Required]
        public string WorkerId { get; set; } = null!;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        [Range(0, double.MaxValue)]
        public decimal LaborCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<CreateJobPartVM> Parts { get; set; } = new List<CreateJobPartVM>();
    }
}
