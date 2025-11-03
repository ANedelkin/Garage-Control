using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CarMake
    {
        public CarMake()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(CarMakeConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        public string? CreatorId { get; set; }
        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; } = null!;
    }
}