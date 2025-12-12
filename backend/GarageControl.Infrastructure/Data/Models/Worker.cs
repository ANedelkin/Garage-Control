using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;
using Microsoft.Identity.Client;
namespace GarageControl.Infrastructure.Data.Models
{
    public class Worker
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(WorkerConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
        public ICollection<JobType> Activities { get; set; } = new HashSet<JobType>();
        public ICollection<Job> Jobs { get; set; } = new HashSet<Job>();
        public ICollection<WorkerSchedule> Schedules { get; set; } = new HashSet<WorkerSchedule>();
        public ICollection<WorkerLeave> Leaves { get; set; } = new HashSet<WorkerLeave>();
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