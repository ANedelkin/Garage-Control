using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class JobPart
    {
        [Required]
        public string JobId { get; set; } = null!;
        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; } = null!;
        [Required]
        public string PartId { get; set; } = null!;
        [ForeignKey(nameof(PartId))]
        public Part Part { get; set; } = null!;
        [Required]
        public double PlannedQuantity { get; set; }
        [Required]
        public double SentQuantity { get; set; }
        [Required]
        public double UsedQuantity { get; set; }
        [Required]
        public double RequestedQuantity { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}
