namespace Homecare.Application.DTOs.PaymentsAndTransactions;

public class UserPaymentExportDto
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = null!;
    public string BookingId { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime? TransactionDateTime { get; set; }
}