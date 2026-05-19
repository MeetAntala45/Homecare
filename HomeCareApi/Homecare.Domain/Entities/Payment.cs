using Homecare.Domain.Enums;

namespace Homecare.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = null!;
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? InvoicePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Booking Booking { get; set; } = null!;
}