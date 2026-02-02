namespace GarageControl.Core.ViewModels.Notifications
{
    public class NotificationViewModel
    {
        public string Id { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
