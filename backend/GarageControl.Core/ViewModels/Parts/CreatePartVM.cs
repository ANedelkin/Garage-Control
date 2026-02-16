using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Parts
{
    public class CreatePartVM
    {
        [Required]
        [StringLength(PartConstants.NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(PartConstants.PartNumberMaxLength)]
        public string PartNumber { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double MinimumQuantity { get; set; }

        public string? ParentId { get; set; }
    }
}
