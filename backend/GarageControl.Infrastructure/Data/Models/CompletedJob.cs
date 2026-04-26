using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CompletedJob
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string CompletedOrderId { get; set; } = null!;
        [ForeignKey(nameof(CompletedOrderId))]
        public CompletedOrder CompletedOrder { get; set; } = null!;

        public string? JobTypeId { get; set; }
        public string? WorkerId { get; set; }

        public string JobTypeName { get; set; } = null!;
        public string? Description { get; set; }
        public string MechanicName { get; set; } = null!;
        [Column(TypeName = "decimal(18,2)")]
        public decimal LaborCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ICollection<CompletedJobPart> CompletedJobParts { get; set; } = new HashSet<CompletedJobPart>();
    }
}
