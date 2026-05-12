using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Parts
{
    public class CreatePartVM
    {
        [StringLength(PartConstants.NameMaxLength, ErrorMessage = "Part name cannot exceed {1} characters.")]
        public string? Name { get; set; }

        [StringLength(PartConstants.PartNumberMaxLength, ErrorMessage = "Part number cannot exceed {1} characters.")]
        public string? PartNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int? Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum quantity cannot be negative.")]
        public int? MinimumQuantity { get; set; }

        public string? ParentId { get; set; }
    }
}
