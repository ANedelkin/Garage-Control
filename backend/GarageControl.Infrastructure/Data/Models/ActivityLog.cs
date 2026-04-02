using System.ComponentModel.DataAnnotations;

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

        /// <summary>Entity category, e.g. "Worker", "Client", "Job"…</summary>
        public string? LogType { get; set; }

        /// <summary>JSON-serialised <see cref="GarageControl.Core.Models.ActivityLogData"/> payload.</summary>
        public string? LogData { get; set; }
    }
}
