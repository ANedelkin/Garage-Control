using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Jobs
{
    public class CreateJobPartVM
    {
        [Required]
        public string PartId { get; set; } = null!;
        [Range(0, int.MaxValue)]
        public int PlannedQuantity { get; set; }
        [Range(0, int.MaxValue)]
        public int SentQuantity { get; set; }
        [Range(0, int.MaxValue)]
        public int UsedQuantity { get; set; }
        [Range(0, int.MaxValue)]
        public int RequestedQuantity { get; set; }
    }
}
