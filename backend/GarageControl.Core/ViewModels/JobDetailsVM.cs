using GarageControl.Shared.Enums;

namespace GarageControl.Core.ViewModels
{
    public class JobDetailsVM
    {
        public string Id { get; set; } = null!;
        public string JobTypeId { get; set; } = null!;
        public string WorkerId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public JobStatus Status { get; set; }
        public decimal LaborCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<JobPartDetailsVM> Parts { get; set; } = new List<JobPartDetailsVM>();
    }
}
