using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class JobPartSnapshot
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string JobSnapshotId { get; set; } = null!;
        [ForeignKey(nameof(JobSnapshotId))]
        public JobSnapshot JobSnapshot { get; set; } = null!;

        public string? JobPartId { get; set; }

        public string? PartId { get; set; }

        public string PartName { get; set; } = null!;
        public int UsedQuantity { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}
