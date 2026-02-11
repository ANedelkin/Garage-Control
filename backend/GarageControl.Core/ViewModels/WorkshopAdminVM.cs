namespace GarageControl.Core.ViewModels
{
    public class WorkshopAdminVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string RegistrationNumber { get; set; } = null!;
        public string OwnerEmail { get; set; } = null!;
        public bool IsBlocked { get; set; }
    }
}
