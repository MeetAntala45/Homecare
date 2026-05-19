namespace Homecare.Application.DTOs.Dashboard;

public class MetricCardResponseDto
{
    public decimal Value { get; set; }
    public decimal Change { get; set; }        
    public bool IsIncrease { get; set; }
}
