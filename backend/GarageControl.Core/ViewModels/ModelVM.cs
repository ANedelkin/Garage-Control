using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels
{
    public class ModelVM
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(CarModelConstants.nameMaxLength, MinimumLength = CarModelConstants.nameMinLength)]
        public string Name { get; set; } = null!;

        public string MakeId { get; set; } = null!;
        public string? MakeName { get; set; }
        public MakeVM? Make { get; set; }
        public bool IsCustom { get; set; }
        public string? GlobalId { get; set; }
    }
}
