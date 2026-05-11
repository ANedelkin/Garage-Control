using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;
using GarageControl.Core.ViewModels.Vehicles;

namespace GarageControl.Core.ViewModels.Clients
{
    public class ClientVM
    {
        public string? Id { get; set; }
        
        public List<VehicleVM>? Cars { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(ClientConstants.nameMaxLength, MinimumLength = ClientConstants.nameMinLength, ErrorMessage = "Name must be between {2} and {1} characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(GenericConstants.phoneMaxLength, ErrorMessage = "Phone number cannot exceed {1} characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = null!;

        [StringLength(GenericConstants.emailMaxLength, ErrorMessage = "Email cannot exceed {1} characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [StringLength(GenericConstants.addressMaxLength)]
        public string? Address { get; set; }

        [StringLength(ClientConstants.registrationNumberMaxLength, ErrorMessage = "Registration number cannot exceed {1} characters.")]
        public string? RegistrationNumber { get; set; }
    }
}
