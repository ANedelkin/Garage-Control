using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Notification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = null!;
        [MaxLength(500)]
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
