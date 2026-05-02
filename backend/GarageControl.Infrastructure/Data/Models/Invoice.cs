using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string OrderId { get; set; } = null!;

        [Required]
        public string WorkshopId { get; set; } = null!;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // The formatted number can be a computed property or stored
        public string InvoiceNumber { get; set; } = null!;
    }
}
