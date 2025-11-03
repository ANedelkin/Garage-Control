using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

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
        public int Quantity { get; set; }
        [Required]
        public string CarServiceId { get; set; } = null!;
        [ForeignKey(nameof(CarServiceId))]
        public CarService CarService { get; set; } = null!;
        [Required]
        public string ParentId { get; set; } = null!;
        [ForeignKey(nameof(ParentId))]
        public PartsFolder Parent { get; set; } = null!;
    }
}
