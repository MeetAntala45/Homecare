namespace Homecare.Application.DTOs.ErrorLogs;

public class ErrorLogCreateDto
{
    public string ExceptionType { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? StackTrace { get; set; }
    public string Path { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public int StatusCode { get; set; }
    public int? UserId { get; set; }
    public string? UserRole { get; set; }
    public DateTime OccurredAt { get; set; }
}