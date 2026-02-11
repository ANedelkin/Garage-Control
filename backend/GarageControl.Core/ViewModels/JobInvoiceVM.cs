namespace GarageControl.Core.ViewModels
{
    public class JobInvoiceVM
    {
        public string JobTypeName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string MechanicName { get; set; } = null!;
        public decimal LaborCost { get; set; }
        public List<JobPartDetailsVM> Parts { get; set; } = new List<JobPartDetailsVM>();
    }
}
