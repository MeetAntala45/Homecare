
namespace Homecare.Application.DTOs.Checkout;

public class ApplyCouponRequestDto
{
    public string CouponCode { get; set; } = null!;
    public int ServiceId { get; set; }
    public int AddressId { get; set; } 
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
}
