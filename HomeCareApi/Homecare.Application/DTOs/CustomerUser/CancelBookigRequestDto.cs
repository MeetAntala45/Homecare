namespace Homecare.Application.DTOs.CustomerUser;
public class CancelBookingRequestDto
{
    public int BookingId {get; set;}
    public string? Reason {get;set;}
}