namespace GarageControl.Core.ViewModels.Orders
{
    public class OrderListVM
    {
        public string Id { get; set; } = null!;
        public string CarId { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Kilometers { get; set; }
        public bool IsDone { get; set; }
    }
}
