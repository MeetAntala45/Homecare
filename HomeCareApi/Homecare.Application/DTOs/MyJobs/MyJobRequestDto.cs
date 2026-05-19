namespace Homecare.Application.DTOs.MyJobs;

public class MyJobRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public string? Status { get; set; }
    public string? ServiceName { get; set; }
    public string? CustomerName { get; set; }
    public DateOnly? BookingDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? PaymentMethod {get; set;}
}
