using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CarModel
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(CarModelConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        public string CarMakeId { get; set; } = null!;
        [ForeignKey(nameof(CarMakeId))]
        public CarMake CarMake { get; set; } = null!;
        [Required]
        public string CreatorId { get; set; } = null!;
        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; } = null!;
    }
}