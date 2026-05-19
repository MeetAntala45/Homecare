using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.AdminUser;

namespace Homecare.Application.Interfaces.AdminUserManagement;

public interface IAdminManagementService
{
    Task<ApiResponse<PagedResult<AdminListItemDto>>> GetAdminListAsync(AdminListFilterDto filter);
    Task<ApiResponse<int>> SaveAdminUserAsync(AdminDto dto, int currentUser);
    Task<ApiResponse<bool>> DeleteAdminUserAsync(int id, int currentUser);
    Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto dto, int currentUser);
    Task<ApiResponse<AdminListItemDto>> GetAdminByIdAsync(int id);
}
