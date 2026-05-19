using System;
using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.ServicePartnerAuth;

public class VerifyPartnerOtpRequest
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150, ErrorMessage = "Email must not exceed 150 characters.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "OTP is required.")]
    public string OtpCode { get; set; } = null!;
}
