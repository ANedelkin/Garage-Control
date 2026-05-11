using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Auth
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(AuthConstants.usernameMinLength, ErrorMessage = "Username too short.")]
        [MaxLength(AuthConstants.usernameMaxLength, ErrorMessage = "Username too long.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(AuthConstants.passwordMinLength, ErrorMessage = "Password too short.")]
        [MaxLength(AuthConstants.passwordMaxLength, ErrorMessage = "Password too long.")]
        public string Password { get; set; } = null!;
    }
}
