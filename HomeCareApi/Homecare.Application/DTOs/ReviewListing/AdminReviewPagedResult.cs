namespace Homecare.Application.DTOs.ReviewListing;

public class AdminReviewPagedResult
{
    public List<AdminReviewListDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}