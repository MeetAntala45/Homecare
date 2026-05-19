namespace Homecare.Application.DTOs.Checkout;

public class PaymentCallbackDto
{
    public int BookingId { get; set; }
    public string TransactionId { get; set; } = null!;
    public string Status { get; set; } = null!;
}