using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyServices;

namespace Homecare.Application.Interfaces.MyService;

public interface IMyService
{
    public Task<ApiResponse<List<PartnerServiceTypeHierarchyResponseDto>>> GetPartnerServiceHierarchyAsync(int partnerId);
    Task<ApiResponse<string>> AddSkillAndServiceAsync(int partnerId, AddPartnerSkillAndServiceRequestDto request);

    Task<ApiResponse<string>> RemoveSkillServiceAsync(int partnerId, int subCategoryId);
}
