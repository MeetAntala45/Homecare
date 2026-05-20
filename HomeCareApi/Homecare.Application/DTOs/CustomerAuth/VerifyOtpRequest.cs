using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CustomerAuth;

public class VerifyOtpRequest
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "OTP is required.")]
    public string OtpCode { get; set; } = null!;

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public string? ReferralCode { get; set; }
}