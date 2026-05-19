namespace Homecare.Application.DTOs.PartnerDashboard;

public class GetPartnerChartRequest
{
    public string Period { get; set; } = "week";
    public string? Week { get; set; }
}