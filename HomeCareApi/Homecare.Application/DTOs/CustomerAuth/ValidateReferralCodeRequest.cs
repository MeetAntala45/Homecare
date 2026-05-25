using System.ComponentModel.DataAnnotations;

namespace Homecare.Application.DTOs.CustomerAuth;

public class ValidateReferralCodeRequest
{
    [Required]
    [MaxLength(20)]
    public string ReferralCode { get; set; } = null!;
}