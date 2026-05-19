namespace Homecare.Application.DTOs.Checkout;

public class UpdateBookingSlotRequestDto
{
    public int BookingId { get; set; }
    public DateOnly NewSlotDate { get; set; }
    public TimeOnly NewSlotStartTime { get; set; }
    public TimeOnly NewSlotEndTime { get; set; }
}