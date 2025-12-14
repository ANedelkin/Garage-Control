using System.ComponentModel.DataAnnotations;
using GarageControl.Infrastructure.Data.Enums;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Access
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(AccessConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        public ICollection<Worker> Workers { get; set; } = new HashSet<Worker>();
    }
}