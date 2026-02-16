namespace GarageControl.Core.ViewModels.Parts
{
    public class LowStockPartVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double CurrentQuantity { get; set; }
        public double MinimumQuantity { get; set; }
    }
}
