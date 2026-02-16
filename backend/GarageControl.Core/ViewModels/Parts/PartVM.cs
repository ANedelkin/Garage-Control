namespace GarageControl.Core.ViewModels.Parts
{
    public class PartVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PartNumber { get; set; } = null!;
        public decimal Price { get; set; }
        public double Quantity { get; set; }
        public double AvailabilityBalance { get; set; }
        public double PartsToSend { get; set; }
        public double MinimumQuantity { get; set; }
        public string? ParentId { get; set; }
        public int DeficitStatus { get; set; } = 0; // DeficitStatus enum: 0=NoDeficit, 1=LowerSeverity, 2=HigherSeverity
    }
}
