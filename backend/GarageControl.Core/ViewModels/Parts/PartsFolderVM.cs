namespace GarageControl.Core.ViewModels.Parts
{
    public class PartsFolderVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ParentId { get; set; }
        public int LowerDeficitSeverityCount { get; set; } = 0;
        public int HigherDeficitSeverityCount { get; set; } = 0;
    }
}
