using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Order
    {
        public Order()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        public string CarId { get; set; } = null!;
        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; } = null!;
        public ICollection<Job> Jobs { get; set; } = new HashSet<Job>();

    }
}