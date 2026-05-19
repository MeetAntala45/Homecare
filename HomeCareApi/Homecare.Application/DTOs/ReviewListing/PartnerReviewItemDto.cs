namespace Homecare.Application.DTOs.ReviewListing;

public class PartnerReviewItemDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerProfileImage { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
}
