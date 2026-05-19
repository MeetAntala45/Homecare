namespace Homecare.Application.DTOs.Notifications;

public class BookingNotificationDto
{
    public int BookingId { get; set; }
    public int PartnerId { get; set; } 
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public int PaymentMethodValue { get; set; }
    public string SlotDate { get; set; } = null!;
    public string SlotTime { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Message { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending";
}