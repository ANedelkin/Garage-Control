using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;
using Microsoft.VisualBasic;

namespace GarageControl.Core.Models
{
    public class ServiceVM
    {
        [Required]
        [MinLength(CarServiceConstants.nameMinLength)]
        [MaxLength(CarServiceConstants.nameMaxLength)]
        public string ServiceName { get; set; } = string.Empty;
        [Required]
        [MinLength(GenericConstants.addressMinLength)]
        [MaxLength(GenericConstants.addressMaxLength)]
        public string Address { get; set; } = string.Empty;
        [Required]
        [MinLength(CarServiceConstants.registrationNumberMinLength)]
        [MaxLength(CarServiceConstants.registrationNumberMaxLength)]
        public string RegistrationNumber { get; set; } = string.Empty;
    }
}