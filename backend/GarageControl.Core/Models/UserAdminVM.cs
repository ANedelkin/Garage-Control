using System;

namespace GarageControl.Core.Models
{
    public class UserAdminVM
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? WorkshopName { get; set; }
    }
}

