using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CarMake
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(CarMakeConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        public string CreatorId { get; set; } = null!;
        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; } = null!;
        public ICollection<CarModel> CarModels { get; set; } = new HashSet<CarModel>();
    }
}