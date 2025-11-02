using System.ComponentModel.DataAnnotations;
using GarageControl.Infrastructure.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace GarageControl.Infrastructure.Data.Models
{
    public class User : IdentityUser
    {
        [Required]
        public UserType UserType { get; set; }
    }
}