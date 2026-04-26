using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CompletedJobPart
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string CompletedJobId { get; set; } = null!;
        [ForeignKey(nameof(CompletedJobId))]
        public CompletedJob CompletedJob { get; set; } = null!;

        public string? PartId { get; set; }

        public string PartName { get; set; } = null!;
        public int UsedQuantity { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}
