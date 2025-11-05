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
        public int Quantity { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}
