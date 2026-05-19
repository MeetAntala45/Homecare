using System.Security.Claims;
using Homecare.Application.Constants;
using Homecare.Application.DTOs;
using Homecare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers;

[ApiController]
[Authorize]
[Route("api/services")]
public class ServiceManagementController : ControllerBase
{
    private readonly IServiceManagementService _serviceManagementService;
    private readonly ICurrentUserService _currentUser;
    private int CurrentUserId => _currentUser.UserId;
    public ServiceManagementController(IServiceManagementService serviceManagementService, ICurrentUserService currentUser)
    {
        _serviceManagementService = serviceManagementService;
        _currentUser = currentUser;
    }

    [HttpGet("get/categories/{categoryId}")]
    public async Task<ApiResponse<List<ServiceResponseDto>>> GetServicesByCategory(
        int categoryId,
        [FromQuery] int? subCategoryId,
        [FromQuery] decimal? priceMin,
        [FromQuery] decimal? priceMax,
        [FromQuery] bool? isAvailable,
        [FromQuery] decimal? commissionPct
    )
    {
        var filter = new ServiceFilterDto
        {
            SubCategoryId = subCategoryId,
            PriceMin = priceMin,
            PriceMax = priceMax,
            IsAvailable = isAvailable,
            CommissionPct = commissionPct,
        };

        return await _serviceManagementService.GetServicesByCategoryAsync(categoryId, filter);
    }

    [HttpGet("get/{id}")]
    [AllowAnonymous]
    public async Task<ApiResponse<ServiceResponseDto>> GetServiceById(int id)
    {
        return await _serviceManagementService.GetServiceByIdAsync(id);
    }

    [HttpGet("get/subcategory/{id}")]
    [AllowAnonymous]
    public async Task<ApiResponse<List<ServiceResponseDto>>> GetServiceBySubCategoryId(int id)
    {
        return await _serviceManagementService.GetServiceBySubCategoryIdAsync(id);
    }

    [HttpGet("get/service-type/{id}")]
    [AllowAnonymous]
    public async Task<ApiResponse<List<ServiceResponseDto>>> GetServiceByServiceTypeId(int id,[FromQuery] string? search = null)
    {
        return await _serviceManagementService.GetServiceByServiceTypeIdAsync(id, search);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromForm] UpsertServiceRequestDto dto)
    {

        var result = await _serviceManagementService.UpsertServiceAsync(dto, CurrentUserId);
        return Ok(result);
    }

    [HttpPatch("edit/{id}/availability")]
    public async Task<ApiResponse<bool>> UpdateAvailability(
        int id,
        [FromBody] UpdateAvailabilityRequestDto dto
    )
    {
        return await _serviceManagementService.UpdateAvailabilityAsync(
            id,
            dto.IsAvailable,
            CurrentUserId
        );
    }

    [HttpDelete("delete/{id}")]
    public async Task<ApiResponse<bool>> DeleteService(int id)
    {
        return await _serviceManagementService.DeleteServiceAsync(id, CurrentUserId);
    }

    [AllowAnonymous]
    [HttpGet("all")]
    public async Task<ApiResponse<List<ServiceResponseDto>>> GetAllService()
    {
        return await _serviceManagementService.GetAllServices();
    }
}
