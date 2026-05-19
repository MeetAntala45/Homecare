namespace Homecare.Application.DTOs.Checkout;

public class GetSlotsRequestDto
{
    public int ServiceId { get; set; }
    public DateOnly Date { get; set; }
    public string Session { get; set; } = "Morning";
    public int AddressId { get; set; }

}