namespace Homecare.Application.DTOs.PaymentsAndTransactions;

public class PaymentListFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    
    public string? UserName { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? PaymentMethod { get; set; }
}