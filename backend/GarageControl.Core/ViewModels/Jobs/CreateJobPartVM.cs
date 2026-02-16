using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Jobs
{
    public class CreateJobPartVM
    {
        [Required]
        public string PartId { get; set; } = null!;
        [Range(0, double.MaxValue)]
        public double PlannedQuantity { get; set; }
        [Range(0, double.MaxValue)]
        public double SentQuantity { get; set; }
        [Range(0, double.MaxValue)]
        public double UsedQuantity { get; set; }
        [Range(0, double.MaxValue)]
        public double RequestedQuantity { get; set; }
    }
}
