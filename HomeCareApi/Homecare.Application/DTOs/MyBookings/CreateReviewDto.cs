namespace Homecare.Application.DTOs.MyBookings;

public class CreateReviewDto
{
    public int BookingId { get; set; }
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
}
