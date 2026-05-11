using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Parts
{
    public class CreatePartVM
    {
        [Required(ErrorMessage = "Part name is required.")]
        [StringLength(PartConstants.NameMaxLength, ErrorMessage = "Part name cannot exceed {1} characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Part number is required.")]
        [StringLength(PartConstants.PartNumberMaxLength, ErrorMessage = "Part number cannot exceed {1} characters.")]
        public string PartNumber { get; set; } = null!;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int? Quantity { get; set; }

        [Required(ErrorMessage = "Minimum quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum quantity cannot be negative.")]
        public int? MinimumQuantity { get; set; }

        public string? ParentId { get; set; }
    }
}
