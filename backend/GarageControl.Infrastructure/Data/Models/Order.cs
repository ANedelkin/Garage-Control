using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string CarId { get; set; } = null!;
        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; } = null!;
        public int Kilometers { get; set; }
        public bool IsDone { get; set; }
        public ICollection<Job> Jobs { get; set; } = new HashSet<Job>();

    }
}