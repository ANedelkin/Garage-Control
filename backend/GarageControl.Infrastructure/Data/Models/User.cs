using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GarageControl.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace GarageControl.Infrastructure.Data.Models
{
    public class User : IdentityUser
    {
        [Required]
        public UserType UserType { get; set; }
        public ICollection<CarMake> CarMakes { get; set; } = new HashSet<CarMake>();
        public ICollection<CarModel> CarModels { get; set; } = new HashSet<CarModel>();
        public string? CarServiceId { get; set; }
        [ForeignKey(nameof(CarServiceId))]
        public CarService? CarService { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}