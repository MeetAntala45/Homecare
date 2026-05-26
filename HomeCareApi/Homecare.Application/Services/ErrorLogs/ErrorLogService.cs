using Homecare.Application.DTOs.ErrorLogs;
using Homecare.Application.Interfaces.ErrorLogs;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Homecare.Application.Services.ErrorLogs;

public class ErrorLogService : IErrorLogService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(
        IServiceScopeFactory scopeFactory,
        ILogger<ErrorLogService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task TrySaveAsync(ErrorLogCreateDto dto)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entry = new ErrorLog
            {
                ExceptionType = Truncate(dto.ExceptionType, 200),
                Message = Truncate(dto.Message, 2000),
                StackTrace = dto.StackTrace,
                Path = Truncate(dto.Path, 500),
                HttpMethod = Truncate(dto.HttpMethod, 10),
                StatusCode = dto.StatusCode,
                UserId = dto.UserId,
                UserRole = dto.UserRole,
                OccurredAt = dto.OccurredAt
            };

            db.ErrorLogs.Add(entry);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            _logger.LogError(ex,
                "ErrorLogService failed to persist error log. " +
                "Original error: {OriginalMessage} | Path: {Path}",
                dto.Message, dto.Path);
        }
    }

    public async Task<ErrorLogPagedResult> GetPagedAsync(ErrorLogFilterDto filter)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var query = db.ErrorLogs.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(e => e.OccurredAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(e => e.OccurredAt <= filter.ToDate.Value.AddDays(1));

        if (filter.StatusCode.HasValue)
            query = query.Where(e => e.StatusCode == filter.StatusCode.Value);

        if (!string.IsNullOrWhiteSpace(filter.ExceptionType))
            query = query.Where(e => e.ExceptionType
                .ToLower().Contains(filter.ExceptionType.ToLower().Trim()));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.ToLower().Trim();
            query = query.Where(e =>
                e.Message.ToLower().Contains(term) ||
                e.Path.ToLower().Contains(term));
        }

        bool isDesc = string.IsNullOrWhiteSpace(filter.SortOrder) ||
                      filter.SortOrder.ToLower() == "desc";

        query = filter.SortBy?.ToLower() switch
        {
            "statuscode" => isDesc ? query.OrderByDescending(e => e.StatusCode)
                                      : query.OrderBy(e => e.StatusCode),
            "exceptiontype" => isDesc ? query.OrderByDescending(e => e.ExceptionType)
                                      : query.OrderBy(e => e.ExceptionType),
            "path" => isDesc ? query.OrderByDescending(e => e.Path)
                                      : query.OrderBy(e => e.Path),
            "message" => isDesc ? query.OrderByDescending(e => e.Message)
                                      : query.OrderBy(e => e.Message),
            _ => isDesc ? query.OrderByDescending(e => e.OccurredAt)
                                      : query.OrderBy(e => e.OccurredAt)
        };

        int totalCount = await query.CountAsync();

        var data = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new ErrorLogListDto
            {
                Id = e.Id,
                ExceptionType = e.ExceptionType,
                Message = e.Message,
                Path = e.Path,
                HttpMethod = e.HttpMethod,
                StatusCode = e.StatusCode,
                UserId = e.UserId,
                UserRole = e.UserRole,
                OccurredAt = e.OccurredAt
            })
            .ToListAsync();

        return new ErrorLogPagedResult { Data = data, TotalCount = totalCount };
    }

    public async Task<ErrorLogDetailDto?> GetByIdAsync(int id)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.ErrorLogs
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new ErrorLogDetailDto
            {
                Id = e.Id,
                ExceptionType = e.ExceptionType,
                Message = e.Message,
                StackTrace = e.StackTrace,
                Path = e.Path,
                HttpMethod = e.HttpMethod,
                StatusCode = e.StatusCode,
                UserId = e.UserId,
                UserRole = e.UserRole,
                OccurredAt = e.OccurredAt
            })
            .FirstOrDefaultAsync();
    }

    private static string Truncate(string? value, int maxLength)
        => value?.Length > maxLength ? value[..maxLength] : value ?? string.Empty;
}