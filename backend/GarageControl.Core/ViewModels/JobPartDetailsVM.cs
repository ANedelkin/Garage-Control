namespace GarageControl.Core.ViewModels
{
    public class JobPartDetailsVM
    {
        public string PartId { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public double PlannedQuantity { get; set; }
        public double SentQuantity { get; set; }
        public double UsedQuantity { get; set; }
        public double RequestedQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
