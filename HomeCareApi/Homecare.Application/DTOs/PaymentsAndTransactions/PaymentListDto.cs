namespace Homecare.Application.DTOs.PaymentsAndTransactions;

public class PaymentListDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string TransactionId { get; set; } = null!;
    public string BookingId { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime CreatedOn { get; set; } 
}