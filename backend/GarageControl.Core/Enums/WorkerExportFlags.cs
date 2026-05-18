namespace GarageControl.Core.Enums
{
    [System.Flags]
    public enum WorkerExportFlags
    {
        None = 0,
        Details = 1 << 0,
        Schedules = 1 << 1,
        Leaves = 1 << 2,
        All = Details | Schedules | Leaves
    }
}
