using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Models
{
    public class PartsFolder
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(FolderConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        [Required]
        public string CarServiceId { get; set; } = null!;
        [ForeignKey(nameof(CarServiceId))]
        public CarService CarService { get; set; } = null!;
        [Required]
        public string ParentId { get; set; } = null!;
        [ForeignKey(nameof(ParentId))]
        public PartsFolder Parent { get; set; } = null!;
        public ICollection<PartsFolder> FolderChildren { get; set; } = new HashSet<PartsFolder>();
        public ICollection<Part> PartsChildren { get; set; } = new HashSet<Part>();
    }
}