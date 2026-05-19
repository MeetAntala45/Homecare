namespace Homecare.Application.DTOs.Checkout;

public class DateAvailabilityDto
{
    public DateOnly Date { get; set; }
    public bool HasSlot { get; set; }
    public bool AllBooked { get; set; } 
}