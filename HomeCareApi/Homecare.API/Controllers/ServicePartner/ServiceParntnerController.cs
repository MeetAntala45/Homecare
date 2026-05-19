using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.ServicePartner;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.ServicePartner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.ServicePartner;

[ApiController]
[Route("api/service-partner")]
public class ServicePartnerController : ControllerBase
{
    private readonly IServicePartnerService _servicePartnerService;
    private readonly ICurrentUserService _currentUser;

    public ServicePartnerController(IServicePartnerService servicePartnerService, ICurrentUserService currentUser)
    {
        _servicePartnerService = servicePartnerService;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpPost("apply")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<ApiResponse<ServicePartnerResponseDto>> Apply(
    [FromForm] CreateServicePartnerRequestDto dto)
    {
        return await _servicePartnerService.CreateServicePartnerAsync(dto);
    }
    [HttpGet("{id:int}")]
    public async Task<ApiResponse<ServicePartnerDetailResponseDto>> GetById(int id)
    {
        return await _servicePartnerService.GetServicePartnerByIdAsync(id);
    }
    [AllowAnonymous]
    [HttpGet("service-types")]
    public async Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypes()
    {
        return await _servicePartnerService.GetServiceTypesAsync();
    }

    [AllowAnonymous]
    [HttpGet("categories/{serviceTypeId:int}")]
    public async Task<ApiResponse<List<DropdownOptionDto>>> GetCategories(int serviceTypeId)
    {
        return await _servicePartnerService.GetCategoriesByServiceTypeAsync(serviceTypeId);
    }

    [AllowAnonymous]
    [HttpGet("sub-categories/{categoryId:int}")]
    public async Task<ApiResponse<List<DropdownOptionDto>>> GetSubCategories(int categoryId)
    {
        return await _servicePartnerService.GetSubCategoriesByCategoryAsync(categoryId);
    }

    [AllowAnonymous]
    [HttpGet("getAll")]
    public async Task<ApiResponse<FilterPagedResult<ServicePartnerListResponseDto>>> GetAll(
    [FromQuery] servicePartnerFilterDto filter)
    {
        return await _servicePartnerService.GetAllServicePartnersAsync(filter);
    }
    [AllowAnonymous]
    [HttpPatch("{id:int}/status")]
    public async Task<ApiResponse<bool>> UpdateStatus(int id)
    {
        return await _servicePartnerService.UpdateStatusAsync(id);
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}/delete")]
    public async Task<ApiResponse<bool>> Delete(int id)
    {
        return await _servicePartnerService.DeleteServicePartnerAsync(id);
    }
    [AllowAnonymous]
    [HttpPatch("{id:int}/approve")]
    public async Task<ApiResponse<bool>> Approve(int id)
    {

        return await _servicePartnerService.ApproveServicePartnerAsync(id, 1);
    }

    [AllowAnonymous]
    [HttpPatch("{id:int}/reject")]
    public async Task<ApiResponse<bool>> Reject(int id)
    {

        return await _servicePartnerService.RejectServicePartnerAsync(id, 1);
    }
    [AllowAnonymous]
    [HttpGet("{id:int}/assigned-services")]
    public async Task<ApiResponse<PagedResult<PartnerAssignedServiceDto>>> GetAssignedServices(
    int id,
    [FromQuery] PartnerAssignedServiceFilterDto filter)
    {
        return await _servicePartnerService.GetAssignedServicesAsync(id, filter);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ApiResponse<PartnerProfileResponseDto>> GetProfile()
    {
        return await _servicePartnerService.GetPartnerProfileAsync(_currentUser.UserId);
    }

    [Authorize]
    [HttpPut("profile")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<ApiResponse<PartnerProfileResponseDto>> UpdateProfile(
        [FromForm] UpdateServicePartnerProfileRequestDto dto)
    {
        return await _servicePartnerService.UpdatePartnerProfileAsync(_currentUser.UserId, dto);
    }

}