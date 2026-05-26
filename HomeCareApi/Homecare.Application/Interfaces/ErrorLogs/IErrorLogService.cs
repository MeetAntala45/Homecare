using Homecare.Application.DTOs.ErrorLogs;

namespace Homecare.Application.Interfaces.ErrorLogs;

public interface IErrorLogService
{
    Task TrySaveAsync(ErrorLogCreateDto dto);

    Task<ErrorLogPagedResult> GetPagedAsync(ErrorLogFilterDto filter);
    Task<ErrorLogDetailDto?> GetByIdAsync(int id);
}