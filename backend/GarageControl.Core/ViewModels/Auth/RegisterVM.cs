using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.ViewModels.Auth
{
    public class RegisterVM
    {
        [Required]
        [MinLength(AuthConstants.usernameMinLength, ErrorMessage = "Username too short.")]
        [MaxLength(AuthConstants.usernameMaxLength, ErrorMessage = "Username too long.")]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(AuthConstants.passwordMinLength, ErrorMessage = "Password too short.")]
        [MaxLength(AuthConstants.passwordMaxLength, ErrorMessage = "Password too long.")]
        public string Password { get; set; } = null!;
    }
}
