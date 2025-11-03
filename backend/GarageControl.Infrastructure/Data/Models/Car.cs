using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Car
    {
        public Car()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        public string ModelId { get; set; } = null!;
        [ForeignKey(nameof(ModelId))]
        public CarModel Model { get; set; } = null!;
        [Required]
        [MaxLength(CarConstants.registrationNumberMaxLength)]
        public string RegistrationNumber { get; set; } = null!;
        [MaxLength(CarConstants.vinMaxLength)]
        public string? VIN { get; set; }
        [Required]
        public string OwnerId { get; set; } = null!;
        [ForeignKey(nameof(OwnerId))]
        public Client Owner { get; set; } = null!;

    }
}