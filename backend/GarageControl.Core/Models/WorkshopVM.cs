using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.Models
{
    public class WorkshopVM
    {
        [Required]
        [MinLength(WorkshopConstants.nameMinLength)]
        [MaxLength(WorkshopConstants.nameMaxLength)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MinLength(GenericConstants.addressMinLength)]
        [MaxLength(GenericConstants.addressMaxLength)]
        public string Address { get; set; } = string.Empty;
        [Required]
        [MinLength(WorkshopConstants.registrationNumberMinLength)]
        [MaxLength(WorkshopConstants.registrationNumberMaxLength)]
        public string RegistrationNumber { get; set; } = string.Empty;
        [Required]
        [Phone]
        [MaxLength(GenericConstants.phoneMaxLength)]
        public string PhoneNumber { get; set; } = string.Empty;
        [EmailAddress]
        [MaxLength(GenericConstants.emailMaxLength)]
        public string? Email { get; set; }
    }
}
