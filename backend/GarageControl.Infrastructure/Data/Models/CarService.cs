using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CarService
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(CarServiceConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(GenericConstants.addressMaxLength)]
        public string Address { get; set; } = null!;
        [MaxLength(CarServiceConstants.registrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }
        [Required]
        public string BossId { get; set; } = null!;
        [ForeignKey(nameof(BossId))]
        public User Boss { get; set; } = null!;
        public ICollection<Worker> Workers { get; set; } = new HashSet<Worker>();
        public ICollection<JobType> JobTypes { get; set; } = new HashSet<JobType>();
        public ICollection<Client> Clients { get; set; } = new HashSet<Client>();
    }
}