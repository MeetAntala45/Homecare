
namespace Homecare.Application.DTOs.Checkout;

public class CheckoutSummaryRequestDto
{
    public int ServiceId { get; set; }
    public DateOnly? SlotDate { get; set; }
    public TimeOnly? SlotStartTime { get; set; }
    public TimeOnly? SlotEndTime { get; set; }
    public string? CouponCode { get; set; }
}
