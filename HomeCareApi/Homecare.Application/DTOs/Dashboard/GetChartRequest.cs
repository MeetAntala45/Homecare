namespace Homecare.Application.DTOs.Dashboard;

public class GetChartRequest
{
    public string Period { get; set; } = "week";
    public string? Week { get; set; }
}
