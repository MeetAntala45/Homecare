using Homecare.Application.DTOs.Bookings;

public class CustomerBookingDetailPagedDto
{
    public List<CustomerBookingDetailDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}