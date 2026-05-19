namespace Homecare.Application.DTOs.Bookings;

public class CustomerBookingSummaryDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int TotalBookedServices { get; set; }
    public decimal TotalBookingAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public int PaymentMethodValue { get; set; }
}