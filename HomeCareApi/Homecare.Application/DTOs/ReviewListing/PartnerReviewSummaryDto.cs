namespace Homecare.Application.DTOs.ReviewListing;

public class PartnerReviewSummaryDto
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int[] RatingBreakdown { get; set; } = new int[5];
    public List<PartnerReviewItemDto> Reviews { get; set; } = [];
}
