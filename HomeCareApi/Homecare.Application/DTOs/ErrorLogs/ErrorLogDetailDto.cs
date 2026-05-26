namespace Homecare.Application.DTOs.ErrorLogs;

public class ErrorLogDetailDto : ErrorLogListDto
{
    public string? StackTrace { get; set; }
}