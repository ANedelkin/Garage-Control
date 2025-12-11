using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.Models
{
    public class JobTypeVM
    {
        public string? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string Color { get; set; } = null!;
        public List<string> Mechanics { get; set; } = new List<string>();
    }
}
