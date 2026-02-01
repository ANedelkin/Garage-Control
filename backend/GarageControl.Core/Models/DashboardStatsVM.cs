using System.Collections.Generic;

namespace GarageControl.Core.Models
{
    public class DashboardStatsVM
    {
        public int TotalUsers { get; set; }
        public int TotalWorkshops { get; set; }
        public int TotalOrders { get; set; }
        public List<UserAdminVM> RecentUsers { get; set; } = new List<UserAdminVM>();
    }
}
