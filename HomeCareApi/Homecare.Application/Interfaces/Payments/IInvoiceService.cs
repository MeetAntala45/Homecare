namespace Homecare.Application.Interfaces.Payments;

public interface IInvoiceService
{
    Task<string> GenerateAsync(int bookingId);
    Task<string> GetInvoicePathAsync(int bookingId);

}