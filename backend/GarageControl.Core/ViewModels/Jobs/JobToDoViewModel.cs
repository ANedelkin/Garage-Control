using GarageControl.Shared.Enums;
using System;

namespace GarageControl.Core.ViewModels.Jobs
{
    public class JobToDoViewModel
    {
        public string Id { get; set; } = null!;
        public string TypeName { get; set; } = null!;
        public string Description { get; set; } = "";
        public JobStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        // Order/Car context
        public string OrderId { get; set; } = null!;
        public string CarName { get; set; } = null!;
        public string CarRegistrationNumber { get; set; } = null!;
        public string ClientName { get; set; } = null!;
    }
}
