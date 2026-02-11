namespace GarageControl.Core.ViewModels
{
    public class FolderContentVM
    {
        public string? CurrentFolderId { get; set; }
        public string? CurrentFolderName { get; set; }
        public string? ParentFolderId { get; set; }
        public IEnumerable<PartsFolderVM> SubFolders { get; set; } = new List<PartsFolderVM>();
        public List<PartVM> Parts { get; set; } = new List<PartVM>();
    }
}
