namespace Homecare.Application.DTOs.ReviewListing;

public class ServiceReviewSummaryDto
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ServiceReviewItemDto> Reviews { get; set; } = [];
}
