using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Vehicles
{
    public class VehicleVM
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Model is required.")]
        public string ModelId { get; set; } = null!;

        public string? ModelName { get; set; }
        public string? MakeName { get; set; }
        public string? MakeId { get; set; }
        public string? VIN { get; set; }
        public string? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public ModelVM? Model { get; set; }

        [Required(ErrorMessage = "Registration number is required.")]
        [StringLength(CarConstants.registrationNumberMaxLength, MinimumLength = CarConstants.registrationNumberMinLength, ErrorMessage = "Registration number must be between {2} and {1} characters.")]
        public string RegistrationNumber { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "Kilometers cannot be negative.")]
        public int Kilometers { get; set; }
    }
}
