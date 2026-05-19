namespace Homecare.Application.DTOs.Dashboard;

public class RevenueChartResponseDto
{
    public string Period { get; set; } = null!;
    
    public List<RevenueChartDataPointDto> Data { get; set; } = new();
}
