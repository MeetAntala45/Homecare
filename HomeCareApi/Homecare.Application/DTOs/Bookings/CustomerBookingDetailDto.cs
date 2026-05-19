namespace Homecare.Application.DTOs.Bookings;

public class CustomerBookingDetailDto
{
    public int BookingId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ServiceType { get; set; } = null!;
    public string DateTime { get; set; } = null!;
    public string? ExpertName { get; set; }
    public string? ExpertPhoto { get; set; }
    public string BookingStatus { get; set; } = null!;
    public int BookingStatusValue { get; set; }
    public decimal Amount { get; set; }
    public int? PartnerId { get; set; }
    public bool IsPartnerDeleted { get; set; }
    public string? PartnerPhone { get; set; }
}