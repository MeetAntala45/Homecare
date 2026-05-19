using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$",
        ErrorMessage = "Password must be 8-15 chars with letter, number, special char"
    )]
    public string Password { get; set; } = string.Empty;
}