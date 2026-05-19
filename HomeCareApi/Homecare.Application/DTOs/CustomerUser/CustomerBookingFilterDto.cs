
namespace Homecare.Application.DTOs.CustomerUser;
public class CustomerBookingFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
     public int? ServiceTypeId { get; set; }
    public DateOnly? Date { get; set; }
    public string? Time { get; set; }       
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Status { get; set; }

}
