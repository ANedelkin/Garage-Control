public static class Accesses
{
    public enum AccessesEnum
    {
        Dashboard,
        Orders,
        PartsStock,
        Workers,
        JobTypes,
        Clients,
        WorkshopDetails,
        MakesAndModels,
        Cars,
        ActivityLog
    }
    public static readonly Dictionary<AccessesEnum, string> AccessNames = new Dictionary<AccessesEnum, string>{
        { AccessesEnum.Dashboard, "Dashboard" },
        { AccessesEnum.Orders, "Orders" },
        { AccessesEnum.PartsStock, "Parts Stock" },
        { AccessesEnum.Workers, "Workers" },
        { AccessesEnum.JobTypes, "Job Types" },
        { AccessesEnum.Clients, "Clients" },
        { AccessesEnum.WorkshopDetails, "Workshop Details" },
        { AccessesEnum.MakesAndModels, "Makes and Models" },
        { AccessesEnum.Cars, "Cars" },
        { AccessesEnum.ActivityLog, "Activity Log" }
    };
}