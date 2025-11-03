using System.ComponentModel.DataAnnotations;
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
        public ICollection<CarService> CarServices { get; set; } = new HashSet<CarService>();
    }
}