using GarageControl.Core.Enums;

namespace GarageControl.Core.Models
{
    public record ActivityPropertyChange(
        string FieldName, 
        string? OldValue, 
        string? NewValue, 
        string? IdOld = null, 
        string? IdNew = null,
        LogEntityType? TargetEntityType = null);
}
