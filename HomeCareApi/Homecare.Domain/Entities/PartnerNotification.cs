namespace Homecare.Domain.Entities;

public class PartnerNotification
{
    public int Id { get; set; }
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
    
    public Booking Booking { get; set; } = null!;
    public ICollection<PartnerNotificationRead> ReadBy { get; set; } = [];
}
