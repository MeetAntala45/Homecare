namespace Homecare.Application.DTOs.ReviewListing;

public class AdminReviewFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = string.Empty;
    public string SortOrder { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? PartnerName { get; set; }
    public string? ServiceName { get; set; }
    public byte? MinRating { get; set; }
    public byte? MaxRating { get; set; }
}