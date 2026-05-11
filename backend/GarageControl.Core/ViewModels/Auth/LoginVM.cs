using System.ComponentModel.DataAnnotations;

namespace GarageControl.Core.ViewModels.Auth
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
    }
}
