
namespace Homecare.Application.DTOs.Checkout;

public class AvailableCouponDto
{
    public int Id { get; set; }
    public string CouponCode { get; set; } = null!;
    public string? Description { get; set; }
    public string Discount { get; set; } = null!;
    public decimal DiscountPct { get; set; } 
    public bool IsEligible { get; set; } = true;
    public string? IneligibleReason { get; set; }
}
