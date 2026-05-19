namespace Homecare.Application.DTOs.CustomerUser;

public class CustomerListFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "id";
    public string SortOrder { get; set; } = "desc";

    public string? UserName { get; set; }
    public int? MinBookings { get; set; }
    public int? MaxBookings { get; set; }
    public string? Status { get; set; }
    public string? Name { get; set; }
}
