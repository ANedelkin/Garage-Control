using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.Models
{
    public class VehicleVM
    {
        public string? Id { get; set; }

        [Required]
        public string MakeId { get; set; } = null!;

        [Required]
        public string ModelId { get; set; } = null!;

        [Required]
        [StringLength(CarConstants.registrationNumberMaxLength)]
        public string RegistrationNumber { get; set; } = null!;

        [StringLength(CarConstants.vinMaxLength)]
        public string? VIN { get; set; }

        [Required]
        public string OwnerId { get; set; } = null!;

        public string? OwnerName { get; set; }
        
        public ModelVM? Model { get; set; }
    }
}
