using System.ComponentModel.DataAnnotations;
using GarageControl.Infrastructure.Data.Enums;

namespace GarageControl.Infrastructure.Data.Models
{
    public class Role
    {
        public Role()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(RoleConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        public ICollection<Worker> Workers { get; set; } = new HashSet<Worker>();
    }
}