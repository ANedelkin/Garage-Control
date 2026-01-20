using System;

namespace GarageControl.Core.Models
{
    public class WorkshopAdminVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string BossEmail { get; set; } = string.Empty;
        public int WorkerCount { get; set; }
        public bool IsBlocked { get; set; }
    }
}
