using System.Collections.Generic;
using GarageControl.Core.Enums;

namespace GarageControl.Core.Models
{
    public record ActivityLogData(
        LogAction Action,

        string? EntityId,

        string? EntityName,

        string? SecondaryEntityId = null,

        string? SecondaryEntityName = null,

        List<ActivityPropertyChange>? Changes = null,
        
        string? ActorId = null,
        
        string? ActorName = null
    );
}
