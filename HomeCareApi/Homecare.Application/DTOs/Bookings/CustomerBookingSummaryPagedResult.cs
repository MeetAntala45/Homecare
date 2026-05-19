using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.Bookings;

public class CustomerBookingSummaryPagedDto : PagedResult<CustomerBookingSummaryDto>
{
    public int MinBookedServices { get; set; }
    public int MaxBookedServices { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int TotalPages { get; set; }

}