using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels
{
    public class CreateFolderVM
    {
        [Required]
        [StringLength(FolderConstants.nameMaxLength)]
        public string Name { get; set; } = null!;
        public string? ParentId { get; set; }
    }
}
