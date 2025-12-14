using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.Models
{
    public class ClientVM
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(ClientConstants.nameMaxLength, MinimumLength = ClientConstants.nameMinLength)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(GenericConstants.phoneMaxLength)]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        [StringLength(GenericConstants.emailMaxLength)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(GenericConstants.addressMaxLength)]
        public string? Address { get; set; }

        [StringLength(ClientConstants.registrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }
    }
}
