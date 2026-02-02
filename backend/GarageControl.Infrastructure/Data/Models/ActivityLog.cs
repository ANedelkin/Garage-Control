using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class ActivityLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string WorkshopId { get; set; } = null!;

        		[Required]
		public string ActorId { get; set; } = null!;

		public string? ActorTargetId { get; set; }

		[Required]
        [MaxLength(200)]
        public string ActorName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string ActorType { get; set; } = null!; // "Owner" or "Worker"

        [Required]
        [MaxLength(500)]
        public string Action { get; set; } = null!;

        public string? TargetId { get; set; }

        [MaxLength(200)]
        public string? TargetName { get; set; }

        [MaxLength(100)]
        public string? TargetType { get; set; } // "Order", "Client", etc.
    }
}
