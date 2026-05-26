using Homecare.Application.Constants;
using Homecare.Application.DTOs.ErrorLogs;
using Homecare.Application.Interfaces.ErrorLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.ErrorLogs;

[ApiController]
[Route("api/admin/error-logs")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ErrorLogController : ControllerBase
{
    private readonly IErrorLogService _errorLogService;

    public ErrorLogController(IErrorLogService errorLogService)
    {
        _errorLogService = errorLogService;
    }

    [HttpGet]
    public async Task<ApiResponse<ErrorLogPagedResult>> GetPaged(
        [FromQuery] ErrorLogFilterDto filter)
    {
        var result = await _errorLogService.GetPagedAsync(filter);
        return ApiResponse<ErrorLogPagedResult>.SuccessResponse(
            "Error logs fetched.", result);
    }

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<ErrorLogDetailDto>> GetById(int id)
    {
        var result = await _errorLogService.GetByIdAsync(id);

        if (result == null)
            return ApiResponse<ErrorLogDetailDto>.Fail("Error log not found.");

        return ApiResponse<ErrorLogDetailDto>.SuccessResponse(
            "Error log fetched.", result);
    }
}