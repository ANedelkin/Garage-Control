using System.ComponentModel.DataAnnotations;

namespace GarageControl.Infrastructure.Data.Models
{
    public class CompletedOrder
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string WorkshopId { get; set; } = null!;
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

        public ICollection<CompletedJob> CompletedJobs { get; set; } = new HashSet<CompletedJob>();
    }
}
