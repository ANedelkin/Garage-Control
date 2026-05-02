namespace GarageControl.Core.ViewModels.Orders
{
    public class OrderDetailsVM
    {
        public string Id { get; set; } = null!;
        public string CarId { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public int Kilometers { get; set; }
        public bool IsArchived { get; set; }
    }
}
