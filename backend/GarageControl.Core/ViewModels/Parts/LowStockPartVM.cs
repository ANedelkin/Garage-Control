namespace GarageControl.Core.ViewModels.Parts
{
    public class LowStockPartVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int CurrentQuantity { get; set; }
        public int MinimumQuantity { get; set; }
    }
}
