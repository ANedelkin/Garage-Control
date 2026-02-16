using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;
using GarageControl.Shared.Enums;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Part
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(PartConstants.NameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(PartConstants.PartNumberMaxLength)]
        public string PartNumber { get; set; } = null!;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Required]
        public double Quantity { get; set; }
        [Required]
        public double MinimumQuantity { get; set; }
        [Required]
        public double AvailabilityBalance { get; set; }
        public ICollection<JobPart> JobParts { get; set; } = new HashSet<JobPart>();
        public string? WorkshopId { get; set; } = null!;
        [ForeignKey(nameof(WorkshopId))]
        public Workshop? Workshop { get; set; }
        public string? ParentId { get; set; } = null!;
        [ForeignKey(nameof(ParentId))]
        public PartsFolder? Parent { get; set; }
        
        // Deficit tracking
        public DeficitStatus DeficitStatus { get; set; } = DeficitStatus.NoDeficit;
    }
}
