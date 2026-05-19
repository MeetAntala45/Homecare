using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CustomerProfile;

public class VerifyEmailChangeDto
{
    [Required(ErrorMessage = "Email is required.")]
    public string NewEmail { get; set; } = null!;

    [Required(ErrorMessage = "OTP is required.")]
    public string Otp { get; set; } = null!;
}