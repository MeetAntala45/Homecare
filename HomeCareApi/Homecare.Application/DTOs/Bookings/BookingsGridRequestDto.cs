namespace Homecare.Application.DTOs.Bookings;

public class BookingGridRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "customerName";
    public string SortOrder { get; set; } = "asc";
    public string? ServiceType { get; set; }
    public string? UserName { get; set; }
    public string? FromDate { get; set; } 
    public string? FromTime { get; set; } 
    public int? PaymentMethod { get; set; } 
    public int? BookingStatus { get; set; }
    public int? MinBookings { get; set; }
    public int? MaxBookings { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}