namespace Homecare.Application.DTOs.Support.cs;

public class SupportFilterRequest
{
    public string? UserName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public string? SortOrder { get; set; } = "desc";
}
