
namespace Homecare.Application.DTOs.Checkout;

public class ApplyCouponResponseDto
{
    public string CouponCode { get; set; } = null!;
    public decimal DiscountAmount { get; set; }
    public string Message { get; set; } = null!;
}
