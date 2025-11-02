using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CarService
    {
        public CarService()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(CarServiceConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(CarServiceConstants.addressMaxLength)]
        public string Address { get; set; } = null!;
        [Required]
        public string BossId { get; set; } = null!;
        [ForeignKey(nameof(BossId))]
        public User Boss { get; set; } = null!;
        public virtual ICollection<Worker> Workers { get; set; } = new HashSet<Worker>();
    }
}