using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Activity
    {
        public Activity()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(ActivityConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        [RegularExpression(ActivityConstants.colorRegex)]
        public string Color { get; set; } = null!;
        [Required]
        public string CarServiceId { get; set; } = null!;
        [ForeignKey(nameof(CarServiceId))]
        public CarService CarService { get; set; } = null!;
        public ICollection<Worker> Workers { get; set; } = new HashSet<Worker>();
    }
}