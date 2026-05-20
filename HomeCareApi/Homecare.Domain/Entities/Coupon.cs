using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPct { get; set; }
    public CouponStatus Status { get; set; } = CouponStatus.Active;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public ICollection<CouponCondition> Conditions { get; set; } = new List<CouponCondition>();
}