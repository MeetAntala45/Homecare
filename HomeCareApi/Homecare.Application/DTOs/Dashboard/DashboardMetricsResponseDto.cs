namespace Homecare.Application.DTOs.Dashboard;

public class DashboardMetricsResponseDto
{
    public MetricCardResponseDto TotalBookings {get; set;} = null!;
    public MetricCardResponseDto ActiveCustomers {get; set;} = null!;
    public MetricCardResponseDto ActivePartners {get; set;} = null!;
    public MetricCardResponseDto TotalRevenue {get; set;} = null!;
}
