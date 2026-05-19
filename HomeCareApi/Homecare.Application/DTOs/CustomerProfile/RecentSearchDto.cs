namespace Homecare.Application.DTOs.CustomerProfile;

public class RecentSearchDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
