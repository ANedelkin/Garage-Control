using Microsoft.AspNetCore.Identity;

namespace GarageControl.Infrastructure.Data.Models
{
    public class User : IdentityUser
    {
        public ICollection<CarMake> CarMakes { get; set; } = new HashSet<CarMake>();
        public ICollection<CarModel> CarModels { get; set; } = new HashSet<CarModel>();
        public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public Workshop? Workshop { get; set; }
        public Worker? Worker { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}