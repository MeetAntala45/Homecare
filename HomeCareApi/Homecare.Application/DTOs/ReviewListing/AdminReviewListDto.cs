namespace Homecare.Application.DTOs.ReviewListing;

public class AdminReviewListDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string PartnerName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
}