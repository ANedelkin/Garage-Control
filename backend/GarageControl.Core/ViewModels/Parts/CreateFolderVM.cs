using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Parts
{
    public class CreateFolderVM
    {
        [Required(ErrorMessage = "Folder name is required.")]
        [StringLength(FolderConstants.nameMaxLength, ErrorMessage = "Folder name cannot exceed {1} characters.")]
        public string Name { get; set; } = null!;
        public string? ParentId { get; set; }
    }
}
