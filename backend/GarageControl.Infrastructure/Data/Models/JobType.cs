using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class JobType
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(JobTypeConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string WorkshopId { get; set; } = null!;
        [ForeignKey(nameof(WorkshopId))]
        public Workshop Workshop { get; set; } = null!;
        public ICollection<Worker> Workers { get; set; } = new HashSet<Worker>();
        public ICollection<Job> Jobs { get; set; } = new HashSet<Job>();
    }
}