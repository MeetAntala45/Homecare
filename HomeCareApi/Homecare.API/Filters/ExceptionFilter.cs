using Homecare.Application.Common.Exceptions;
using Homecare.Application.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Homecare.API.Filters;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        int statusCode = exception switch
        {
            KeyNotFoundException       => StatusCodes.Status404NotFound,
            ArgumentException          => StatusCodes.Status400BadRequest,
            InvalidOperationException  => StatusCodes.Status409Conflict,
            UnauthorizedException      => StatusCodes.Status401Unauthorized,
            _                          => StatusCodes.Status500InternalServerError
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
                "Handled exception. Type: {ExceptionType} | StatusCode: {StatusCode} | Message: {Message} | Path: {Path}",
                exception.GetType().Name,
                statusCode,
                exception.Message,
                context.HttpContext.Request.Path);
        }

        var response = ApiResponse<string>.Fail(exception.Message);

        context.Result = new ObjectResult(response)
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}