using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.ServicePartner;

namespace Homecare.Application.Interfaces.ServicePartner;

public interface IServicePartnerService
{
    Task<ApiResponse<ServicePartnerResponseDto>> CreateServicePartnerAsync(
        CreateServicePartnerRequestDto dto);

    Task<ApiResponse<FilterPagedResult<ServicePartnerListResponseDto>>> GetAllServicePartnersAsync(
    servicePartnerFilterDto filter);
    Task<ApiResponse<ServicePartnerDetailResponseDto>> GetServicePartnerByIdAsync(int id);
    Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypesAsync();
    Task<ApiResponse<List<DropdownOptionDto>>> GetCategoriesByServiceTypeAsync(int serviceTypeId);
    Task<ApiResponse<List<DropdownOptionDto>>> GetSubCategoriesByCategoryAsync(int categoryId);
    Task<ApiResponse<bool>> UpdateStatusAsync(int id);
    Task<ApiResponse<bool>> DeleteServicePartnerAsync(int id);
    Task<ApiResponse<bool>> ApproveServicePartnerAsync(int id, int adminUserId);
    Task<ApiResponse<bool>> RejectServicePartnerAsync(int id, int adminUserId);
    Task<ApiResponse<PagedResult<PartnerAssignedServiceDto>>> GetAssignedServicesAsync(
    int partnerId,
    PartnerAssignedServiceFilterDto filter);
    Task<ApiResponse<PartnerProfileResponseDto>> GetPartnerProfileAsync(int partnerId);
    Task<ApiResponse<PartnerProfileResponseDto>> UpdatePartnerProfileAsync(int partnerId, UpdateServicePartnerProfileRequestDto dto);
}
