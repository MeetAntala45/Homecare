namespace Homecare.Application.DTOs.PartnerDashboard;

public class PartnerDashboardMetricsResponseDto
{
    public MetricCardResponseDto TotalBookings { get; set; } = null!;
    public MetricCardResponseDto UniqueCustomers { get; set; } = null!;
    public MetricCardResponseDto AverageRating { get; set; } = null!;
    public MetricCardResponseDto TotalRevenue { get; set; } = null!;
}