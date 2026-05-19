using Homecare.Application.DTOs.PaymentsAndTransactions;
using Homecare.Application.Interfaces.PaymentsAndTransactions;
using Homecare.Application.Interfaces.Shared;

namespace Homecare.Application.Services.Shared;

public class UserPaymentExporter : IDataExporter
{
    private readonly IPaymentTransactionService _paymentTransactionService;

    public string Type => "user-payments";

    public UserPaymentExporter(IPaymentTransactionService paymentTransactionService)
    {
        _paymentTransactionService = paymentTransactionService;
    }

    public async Task<byte[]> ExportCsvAsync(Dictionary<string, string> queryParams)
    {
        var (filter, paginate) = BuildFilter(queryParams);
        var data = await _paymentTransactionService.GetAllUserPaymentForExportAsync(filter, paginate);

        var headers = new List<string>
            { "Payment ID", "Transaction ID", "Booking ID", "Service", "Amount", "Payment Method", "Date" };

        var rows = data.Select(p => new List<string>
        {
            p.Id.ToString(),            
            p.TransactionId,
            p.BookingId,
            p.ServiceName,
            $"+${p.Amount:F2}",
            p.PaymentMethod,
            p.TransactionDateTime.HasValue
                ? p.TransactionDateTime.Value.ToString("dd MMM yyyy") 
                : "-"        
        }).ToList();
                
        return CsvExportHelper.Generate(headers, rows);
    }

    public async Task<byte[]> ExportPdfAsync(Dictionary<string, string> queryParams)
    {
        var (filter, paginate) = BuildFilter(queryParams);
        var data = await _paymentTransactionService.GetAllUserPaymentForExportAsync(filter, paginate);

        var headers = new List<string>
            { "Payment ID", "Transaction ID", "Booking ID", "Service", "Amount", "Payment Method", "Date" };

        var columnWeights = new List<float> 
            { 1.4f, 3.5f, 1.5f, 3.0f, 1.5f, 2.5f, 1.8f };

        var rows = data.Select(p => new List<string>
        {
            p.Id.ToString(),
            p.TransactionId,
            p.BookingId,
            p.ServiceName,
            $"+${p.Amount:F2}",
            p.PaymentMethod,
            p.TransactionDateTime.HasValue
                ? p.TransactionDateTime.Value.ToString("dd MMM yyyy") 
                : "-"
        }).ToList();

        return PdfExportHelper.Generate( "Payments & Transactions", headers, rows, columnWeights);
    }

    private (UserPaymentFilterDto filter, bool paginate) BuildFilter(Dictionary<string, string> p)
    {
        var userId = QueryParamParser.GetInt(p, "userId");
        var currentPaymentId = QueryParamParser.GetInt(p, "currentPaymentId");
        var paymentMethod = QueryParamParser.GetString(p, "paymentMethod");
        var minAmount = QueryParamParser.GetDecimal(p, "minAmount");
        var maxAmount = QueryParamParser.GetDecimal(p, "maxAmount");
        var sortBy = QueryParamParser.GetString(p, "sortBy");
        var sortOrder = QueryParamParser.GetString(p, "sortOrder");
        var pageNumber = QueryParamParser.GetInt(p, "pageNumber");
        var pageSize = QueryParamParser.GetInt(p, "pageSize");

        var paginate = pageNumber > 0 && pageSize > 0;

        var filter = new UserPaymentFilterDto
        {
            UserId = userId,
            CurrentPaymentId = currentPaymentId,
            PaymentMethod = paymentMethod,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            SortBy = sortBy,
            SortOrder = sortOrder,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return (filter, paginate);
    }
}