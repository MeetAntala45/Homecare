using Homecare.Application.Constants;
using Homecare.Application.DTOs;

namespace Homecare.Application.Interfaces;

public interface IServiceManagementService
{
    Task<ApiResponse<List<ServiceResponseDto>>> GetAllServices();
    Task<ApiResponse<List<ServiceResponseDto>>> GetServicesByCategoryAsync(
        int categoryId,
        ServiceFilterDto filter);

    Task<ApiResponse<ServiceResponseDto>> GetServiceByIdAsync(int id);
    Task<ApiResponse<List<ServiceResponseDto>>> GetServiceBySubCategoryIdAsync(int id);

    Task<ApiResponse<ServiceResponseDto>> UpsertServiceAsync(
        UpsertServiceRequestDto dto,
        int userId);

    Task<ApiResponse<bool>> UpdateAvailabilityAsync(
        int id,
        bool isAvailable,
        int modifiedBy);

    Task<ApiResponse<bool>> DeleteServiceAsync(int id, int userId);

    Task<ApiResponse<List<ServiceResponseDto>>> GetServiceByServiceTypeIdAsync(int serviceTypeId, string search);
}