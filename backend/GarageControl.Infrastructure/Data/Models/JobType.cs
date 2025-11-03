using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class JobType
    {
        public JobType()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(JobTypeConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        public string Color { get; set; } = null!;
        [Required]
        public string CarServiceId { get; set; } = null!;
        [ForeignKey(nameof(CarServiceId))]
        public CarService CarService { get; set; } = null!;
        public ICollection<Worker> Workers { get; set; } = new HashSet<Worker>();
        public ICollection<Job> Jobs { get; set; } = new HashSet<Job>();
    }
}