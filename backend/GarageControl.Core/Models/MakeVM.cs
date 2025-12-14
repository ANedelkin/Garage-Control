using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.Models
{
    public class MakeVM
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(CarMakeConstants.nameMaxLength, MinimumLength = CarMakeConstants.nameMinLength)]
        public string Name { get; set; } = null!;
    }
}
