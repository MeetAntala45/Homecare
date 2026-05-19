using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;
}
