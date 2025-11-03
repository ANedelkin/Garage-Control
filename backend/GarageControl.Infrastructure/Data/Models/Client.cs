using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Client
    {
        public Client()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(ClientConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(GenericConstants.phoneMaxLength)]
        public string PhoneNumber { get; set; } = null!;
        [MaxLength(GenericConstants.emailMaxLength)]
        public string? Email { get; set; }
        [MaxLength(GenericConstants.registrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }
        [MaxLength(GenericConstants.addressMaxLength)]
        public string? Address { get; set; }
        public ICollection<Car> Cars { get; set; } = new HashSet<Car>();
    }
}