namespace Homecare.Application.DTOs.MyBookings;
public class MyBookingResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }

    public string? ProfileImage { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string? PartnerName { get; set; }

    public string BookingStatus { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string MobileNumber { get; set; } = string.Empty;

    public int Duration { get; set; }

    public string HouseFlatNo { get; set; } = string.Empty;

    public string CancellationReason { get; set; } = string.Empty;

    public string LandMark { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    public bool HasReview { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateOnly BookingDate { get; set; }

    public TimeOnly SlotStartTime { get; set; }
}