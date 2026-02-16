using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Vehicles
{
    public class VehicleVM
    {
        public string? Id { get; set; }

        [Required]
        public string ModelId { get; set; } = null!;

        public string? ModelName { get; set; }
        public string? MakeName { get; set; }
        public string? MakeId { get; set; }
        public string? VIN { get; set; }
        public string? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public ModelVM? Model { get; set; }

        [Required]
        [StringLength(CarConstants.registrationNumberMaxLength, MinimumLength = CarConstants.registrationNumberMinLength)]
        public string RegistrationNumber { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int Kilometers { get; set; }
    }
}
