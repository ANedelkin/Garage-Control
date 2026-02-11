namespace GarageControl.Core.ViewModels
{
    public class OrderDetailsVM
    {
        public string Id { get; set; } = null!;
        public string CarId { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public int Kilometers { get; set; }
        public bool IsDone { get; set; }
        public List<JobDetailsVM> Jobs { get; set; } = new List<JobDetailsVM>();
    }
}
