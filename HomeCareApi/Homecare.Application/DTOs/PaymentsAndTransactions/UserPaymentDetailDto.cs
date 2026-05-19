namespace Homecare.Application.DTOs.PaymentsAndTransactions;

public class UserPaymentDetailDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? MobileNumber { get; set; }
    public string TransactionId { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public int ServiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime TransactionDateTime { get; set; }
}