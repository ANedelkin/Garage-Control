using System;
using System.Collections.Generic;

namespace GarageControl.Core.ViewModels
{
    public class ActivityLogVM
    {
        public string Id { get; set; } = null!;
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; } = null!; // Header: Actor Action ActedOn
        public List<string> Details { get; set; } = new();
    }
}
