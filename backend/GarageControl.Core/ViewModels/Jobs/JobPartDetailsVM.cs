namespace GarageControl.Core.ViewModels.Jobs
{
    public class JobPartDetailsVM
    {
        public string PartId { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public int PlannedQuantity { get; set; }
        public int SentQuantity { get; set; }
        public int UsedQuantity { get; set; }
        public int RequestedQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
