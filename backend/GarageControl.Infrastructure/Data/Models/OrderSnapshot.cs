using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class OrderSnapshot
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string WorkshopId { get; set; } = null!;
        [Required]
        public string OrderId { get; set; } = null!;

        public DateTime CompletionDate { get; set; } = DateTime.UtcNow;

        public string WorkshopName { get; set; } = null!;
        public string WorkshopAddress { get; set; } = null!;
        public string WorkshopPhone { get; set; } = null!;
        public string WorkshopEmail { get; set; } = null!;
        public string WorkshopRegistrationNumber { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public int Kilometers { get; set; }

        public ICollection<JobSnapshot> JobSnapshots { get; set; } = new HashSet<JobSnapshot>();
    }
}
