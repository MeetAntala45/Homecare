namespace Homecare.Application.DTOs.Payments;

public class BookingSuccessResponseDto
{
    public int BookingId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ServiceCategory { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public decimal AmountPaid { get; set; }
    public string Location { get; set; } = null!;
    public string SlotDate { get; set; } = null!;
    public string SlotStartTime { get; set; } = null!;
    public bool PartnerAssigned { get; set; }
    public string? PartnerName { get; set; }
    public string? PartnerImage { get; set; }
    public string? PartnerMobileNumber { get; set; }
    public string? couponCode { get; set; }

    public string? InvoicePath { get; set; } 
    public int ServiceId { get; set; }
    public string? FailureReason { get; set; }
}