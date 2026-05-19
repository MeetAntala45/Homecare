namespace Homecare.Application.DTOs.Payments;

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string TransactionId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public string CreatedAt { get; set; } = null!;
}
