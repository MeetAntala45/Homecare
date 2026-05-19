namespace Homecare.Application.DTOs.Dashboard;

public class TopCityDto
{
    public string City { get; set; } = string.Empty;
    public List<int> Data { get; set; } = new();
}
