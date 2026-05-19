namespace Homecare.Application.DTOs.PartnerDashboard;

public class MetricCardResponseDto
{
    public decimal Value { get; set; }
    public decimal Change { get; set; }
    public bool IsIncrease { get; set; }
}