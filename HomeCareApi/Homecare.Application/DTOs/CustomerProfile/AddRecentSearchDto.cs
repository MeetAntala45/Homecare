namespace Homecare.Application.DTOs.CustomerProfile;

public class AddRecentSearchDto
{
    public string DisplayName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
