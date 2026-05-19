using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.Auth;

public class ResetPasswordRequest
{
    [Required]
    public required string Token { get; set; }
    [Required]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$",
        ErrorMessage = "Password must be 8-15 chars with letter, number, special char"
    )]
    public required string NewPassword { get; set; }
    [Required]
    public required string ConfirmPassword { get; set; }
}
