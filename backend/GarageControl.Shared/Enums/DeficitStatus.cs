namespace GarageControl.Shared.Enums
{
    public enum DeficitStatus
    {
        NoDeficit,
        LowerSeverity,      // Availability balance under minimum (but >= 0)
        HigherSeverity      // Negative availability balance OR stockpiled below minimum
    }
}
