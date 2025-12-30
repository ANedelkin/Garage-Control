using System.ComponentModel.DataAnnotations;
using GarageControl.Shared.Constants;

namespace GarageControl.Core.Models
{
    public class AuthVM
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        [MinLength(AuthConstants.emailMinLength, ErrorMessage = $"Email too short.")]
        [MaxLength(AuthConstants.emailMaxLength, ErrorMessage = $"Email too long.")]
        public string Email { get; set; } = null!;
        [Required]
        [MinLength(AuthConstants.passwordMinLength, ErrorMessage = $"Password too short.")]
        [MaxLength(AuthConstants.passwordMaxLength, ErrorMessage = $"Password too long.")]
        public string? Password { get; set; } = null!;
    }
}