using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Parts
{
    public class PartViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PartNumber { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
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
        public int Quantity { get; set; }
        public string? ParentId { get; set; }
    }

    public class UpdatePartViewModel
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string PartNumber { get; set; } = null!;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
