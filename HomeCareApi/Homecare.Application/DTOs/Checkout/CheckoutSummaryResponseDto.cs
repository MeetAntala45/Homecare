
namespace Homecare.Application.DTOs.Checkout;

public class CheckoutSummaryResponseDto
{
    public string ServiceName { get; set; } = null!;
    public decimal ServicePrice { get; set; }
    public decimal TaxPct { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? AppliedCouponCode { get; set; }
    public decimal TotalAmount { get; set; }
}
