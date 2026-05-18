using System.Collections.Generic;
using GarageControl.Core.Enums;

namespace GarageControl.Core.Models
{
    public record ActivityLogData(
        LogAction Action,

        string? EntityId,

        string? EntityName = null,

        List<ActivityPropertyChange>? Changes = null,
        
        string? ActorId = null,
        
        string? ActorName = null
    );
}
