namespace GarageControl.Core.ViewModels.Orders
{
    using GarageControl.Core.ViewModels.Jobs;

    public class OrderInvoiceVM
    {
        public string OrderId { get; set; } = null!;
        public string WorkshopName { get; set; } = null!;
        public string WorkshopAddress { get; set; } = null!;
        public string WorkshopPhone { get; set; } = null!;
        public string WorkshopEmail { get; set; } = null!;
        public string WorkshopRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public int Kilometers { get; set; }
        public List<JobInvoiceVM> Jobs { get; set; } = new List<JobInvoiceVM>();
    }
}
