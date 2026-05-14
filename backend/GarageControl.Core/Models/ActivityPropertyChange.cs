using GarageControl.Core.Enums;

namespace GarageControl.Core.Models
{
    public record ActivityPropertyChange(
        string FieldName, 
        string? OldValue, 
        string? NewValue, 
        string? OldId = null, 
        string? NewId = null,
        LogEntityType? TargetEntityType = null);
}
