using Homecare.Domain.Enums;

namespace Homecare.Application.DTOs.Checkout;

public class CreateBookingResponseDto
{
    public int BookingId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? CouponCode { get; set; }
    public decimal WalletAmountUsed { get; set; }
    public decimal RefereeDiscount { get; set; }

}