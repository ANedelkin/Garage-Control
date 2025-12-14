using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.Models
{
    public class ModelVM
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(CarModelConstants.nameMaxLength, MinimumLength = CarModelConstants.nameMinLength)]
        public string Name { get; set; } = null!;

        [Required]
        public string MakeId { get; set; } = null!;
    }
}
