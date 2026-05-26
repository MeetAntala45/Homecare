namespace Homecare.Application.DTOs.ErrorLogs;

public class ErrorLogPagedResult
{
    public List<ErrorLogListDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}