using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Parts
{
    public class PartViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PartNumber { get; set; } = null!;
        public decimal Price { get; set; }
        public double Quantity { get; set; }
        public double AvailabilityBalance { get; set; }
        public double PartsToSend { get; set; }
        public double MinimumQuantity { get; set; }
        public string? ParentId { get; set; }
    }

    public class CreatePartViewModel
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string PartNumber { get; set; } = null!;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public double Quantity { get; set; }
        [Required]
        public double MinimumQuantity { get; set; }
        public string? ParentId { get; set; }
    }

    public class UpdatePartViewModel
    {

        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string PartNumber { get; set; } = null!;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public double Quantity { get; set; }
        [Required]
        public double MinimumQuantity { get; set; }
    }
    public class PartWithPathViewModel : PartViewModel
    {
        public List<string> Path { get; set; } = new();
    }
}
