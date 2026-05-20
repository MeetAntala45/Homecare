namespace Homecare.Application.DTOs.CustomerAuth;

public class VerifyOtpResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiry { get; set; }
    public bool IsNewUser { get; set; }
    public string? ReferralCode { get; set; }
}