using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Identity.Client;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Worker
    {
        public Worker()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
        [Required]
        public string CarServiceId { get; set; } = null!;
        [ForeignKey(nameof(CarServiceId))]
        public CarService CarService { get; set; } = null!;
        public DateTime HiredOn { get; set; }
    }
}