namespace Homecare.Application.DTOs.CustomerAuth;

public class ValidateReferralCodeResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}