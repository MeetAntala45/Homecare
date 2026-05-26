namespace Homecare.Application.DTOs.ErrorLogs;

public class ErrorLogFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "occurredAt";
    public string SortOrder { get; set; } = "desc";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? StatusCode { get; set; }
    public string? ExceptionType { get; set; }
    public string? Search { get; set; }
}