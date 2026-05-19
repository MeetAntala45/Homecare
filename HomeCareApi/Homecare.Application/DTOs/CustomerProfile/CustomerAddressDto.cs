namespace Homecare.Application.DTOs.CustomerProfile;

public class CustomerAddressDto
{
    public int Id { get; set; }
    public string HouseFlatNo { get; set; } = null!;
    public string Landmark { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string? DisplayName { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}