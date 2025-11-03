using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;
using GarageControl.Shared.Enums;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Job
    {
        public Job()
        {
            Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        public string JobTypeId { get; set; } = null!;
        [ForeignKey(nameof(JobTypeId))]
        public JobType JobType { get; set; } = null!;
        [MaxLength(JobConstants.descriptionMaxLength)]
        public string? Description { get; set; }
        [Required]
        public JobStatus Status { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LaborCost { get; set; }
        [Required]
        public string WorkerId { get; set; } = null!;
        [ForeignKey(nameof(WorkerId))]
        public Worker Worker { get; set; } = null!;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public string OrderId { get; set; } = null!;
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;
    }
}