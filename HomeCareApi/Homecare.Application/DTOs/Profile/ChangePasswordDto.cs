using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.Profile
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [RegularExpression(
            @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$",
            ErrorMessage = "Password must be 8-15 characters with letter, number and special character")]
        public string NewPassword { get; set; } = string.Empty;
    }
}