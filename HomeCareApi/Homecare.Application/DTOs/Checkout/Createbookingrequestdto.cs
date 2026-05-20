// Homecare.Application/DTOs/Checkout/CreateBookingRequestDto.cs — ADD UseWallet
// (modify existing file)
using Homecare.Domain.Enums;

namespace Homecare.Application.DTOs.Checkout;

public class CreateBookingRequestDto
{
    public int ServiceId { get; set; }
    public int AddressId { get; set; }
    public string? CouponCode { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool UseWallet { get; set; } = false;
}