namespace GarageControl.Core.ViewModels
{
    public class UserAdminVM
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string WorkshopId { get; set; } = null!;
        public string WorkshopName { get; set; } = null!;
        public bool IsBlocked { get; set; }
        public string? Role { get; set; }
    }
}
