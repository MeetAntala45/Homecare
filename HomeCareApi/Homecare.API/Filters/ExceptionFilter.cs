using System.Security.Claims;
using Homecare.Application.Common.Exceptions;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.ErrorLogs;
using Homecare.Application.Interfaces.ErrorLogs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Homecare.API.Filters;

public class ExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;
    private readonly IErrorLogService _errorLogService;

    public ExceptionFilter(
        ILogger<ExceptionFilter> logger,
        IErrorLogService errorLogService)
    {
        _logger = logger;
        _errorLogService = errorLogService;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;

        int statusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception,
                "Unhandled exception. Type: {ExceptionType} | Message: {Message} | Path: {Path}",
                exception.GetType().Name,
                exception.Message,
                context.HttpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception. Type: {ExceptionType} | StatusCode: {StatusCode} | " +
                "Message: {Message} | Path: {Path}",
                exception.GetType().Name,
                statusCode,
                exception.Message,
                context.HttpContext.Request.Path);
        }

        var userIdClaim = context.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.HttpContext.User
            .FindFirst(ClaimTypes.Role)?.Value;

        int? userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : null;

        var dto = new ErrorLogCreateDto
        {
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            Path = context.HttpContext.Request.Path,
            HttpMethod = context.HttpContext.Request.Method,
            StatusCode = statusCode,
            UserId = userId,
            UserRole = userRole,
            OccurredAt = DateTime.UtcNow
        };

        await _errorLogService.TrySaveAsync(dto);

        var response = ApiResponse<string>.Fail(exception.Message);

        context.Result = new ObjectResult(response) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }
}