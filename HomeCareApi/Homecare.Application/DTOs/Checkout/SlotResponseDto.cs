namespace Homecare.Application.DTOs.Checkout;

public class SlotResponseDto
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool Available { get; set; }
}