namespace Homecare.Application.DTOs.CustomerAuth;

public class SendOtpResponse
{
    public int? CooldownSeconds { get; set; } 
    public bool IsRateLimited { get; set; }
}
