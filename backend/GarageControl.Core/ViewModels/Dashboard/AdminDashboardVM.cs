using System.Collections.Generic;
using GarageControl.Core.ViewModels.Workshop;

namespace GarageControl.Core.ViewModels.Dashboard
{
    public class AdminDashboardVM
    {
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalWorkshops { get; set; }
        public List<UserAdminVM> RecentUsers { get; set; } = new();
    }
}
