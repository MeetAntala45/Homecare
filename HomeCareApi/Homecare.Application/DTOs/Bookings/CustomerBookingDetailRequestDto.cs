namespace Homecare.Application.DTOs.Bookings;

public class CustomerBookingDetailRequestDto
{
    public int CustomerId { get; set; }
    public int PaymentMethod { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public string? ServiceType { get; set; }
    public string? FromDate { get; set; }
    public string? FromTime { get; set; }
    public int? BookingStatus { get; set; }
}