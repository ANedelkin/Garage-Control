namespace GarageControl.Core.ViewModels
{
    public class JobListVM
    {
        public string Id { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string MechanicName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal LaborCost { get; set; }
    }
}
