namespace Homecare.Application.DTOs.PartnerDashboard;

public class PartnerRevenueChartResponseDto
{
    public string Period { get; set; } = null!;
    public List<PartnerRevenueChartDataPointDto> Data { get; set; } = new();
}