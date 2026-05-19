
namespace Homecare.Domain.Entities;

public class CouponCondition
{
    public int Id { get; set; }

    public int CouponId { get; set; }

    public int ConditionTypeId { get; set; }

    public string Operator { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string FailBehaviour { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Coupon Coupon { get; set; } = null!;

    public CouponConditionType ConditionType { get; set; } = null!;

}
