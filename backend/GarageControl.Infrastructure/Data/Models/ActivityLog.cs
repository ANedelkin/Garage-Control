using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GarageControl.Infrastructure.Data.Models
{
    public class ActivityLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        [Required]
        public string WorkshopId { get; set; } = null!;
        [Required]
        public string MessageHtml { get; set; } = null!;
    }
}
