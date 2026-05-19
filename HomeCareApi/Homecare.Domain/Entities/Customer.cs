using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? MobileNumber { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public int PendingBookings { get; set; } = 0;
    public int TotalBookings { get; set; } = 0;
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<OtpVerification> OtpVerifications { get; set; } = new List<OtpVerification>();
}
