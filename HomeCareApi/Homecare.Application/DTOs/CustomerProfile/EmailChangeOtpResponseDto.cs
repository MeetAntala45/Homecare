namespace Homecare.Application.DTOs.CustomerProfile;

public class EmailChangeOtpResponseDto
{
    public string Message { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}