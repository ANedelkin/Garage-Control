using System.ComponentModel.DataAnnotations;

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
        public string MessageHtml { get; set; } = null!;
    }
}
