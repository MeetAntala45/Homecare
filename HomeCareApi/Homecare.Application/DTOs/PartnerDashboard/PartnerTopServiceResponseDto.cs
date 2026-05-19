namespace Homecare.Application.DTOs.PartnerDashboard;

public class PartnerTopServiceResponseDto
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = null!;
    public int BookingCount { get; set; }
}