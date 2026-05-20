using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class ReferralUse
{
    public int Id { get; set; }
    public int ReferrerId { get; set; }
    public int RefereeId { get; set; } 
    public string ReferralCode { get; set; } = null!;
    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
    public int? RewardBookingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RewardedAt { get; set; }

    public Customer Referrer { get; set; } = null!;
    public Customer Referee { get; set; } = null!;
}