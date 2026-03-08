using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Auth
{
    public class LoginVM
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
