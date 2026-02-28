namespace GarageControl.Core.ViewModels.Workshop
{
    public class WorkshopAdminVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string OwnerEmail { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int WorkersCount { get; set; }
        public bool IsBlocked { get; set; }
    }
}
