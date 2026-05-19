namespace Homecare.Domain.Entities;

public class PartnerOtpVerification
{
    public int Id { get; set; }
    public int ServicePartnerId { get; set; }
    public ServicePartner? ServicePartner { get; set; }
    public string Email { get; set; } = null!;
    public string OtpCode { get; set; } = null!;
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
}
