using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Parts
{
    public class PartsFolderViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ParentId { get; set; }
    }

    public class CreateFolderViewModel
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? ParentId { get; set; }
    }

    public class FolderContentViewModel
    {
        public string? CurrentFolderId { get; set; }
        public string? CurrentFolderName { get; set; }
        public string? ParentFolderId { get; set; }
        public IEnumerable<PartsFolderViewModel> SubFolders { get; set; } = new List<PartsFolderViewModel>();
        public IEnumerable<PartViewModel> Parts { get; set; } = new List<PartViewModel>();
    }
}
