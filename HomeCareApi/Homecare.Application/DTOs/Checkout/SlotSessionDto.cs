namespace Homecare.Application.DTOs.Checkout;

public class SlotSessionDto
{
    public string SessionName { get; set; }
    public string SessionRange { get; set; }
    public bool HasAvailableSlots { get; set; }
    public List<SlotResponseDto> Slots { get; set; } = new();
}   