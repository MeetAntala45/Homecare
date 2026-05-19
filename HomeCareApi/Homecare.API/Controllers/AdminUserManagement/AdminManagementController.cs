using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.AdminUser;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.AdminUserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.AdminUserManagement
{
    [Route("api/admin")]
    [ApiController]
    public class AdminManagementController : ControllerBase
    {
        private readonly IAdminManagementService _service;
        private readonly ICurrentUserService _currentUser;

        public AdminManagementController(IAdminManagementService service, ICurrentUserService currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet("admin-list")]
        public async Task<ApiResponse<PagedResult<AdminListItemDto>>> GetAdminList(
            [FromQuery] AdminListFilterDto filter)
        {
            return await _service.GetAdminListAsync(filter);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("save")]
        public async Task<ApiResponse<int>> Save(AdminDto dto)
        {
            return await _service.SaveAdminUserAsync(dto, _currentUser.UserId);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("delete-admin/{id}")]
        public async Task<ApiResponse<bool>> DeleteAdmin(int id)
        {
            return await _service.DeleteAdminUserAsync(id, _currentUser.UserId);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("change-password")]
        public async Task<ApiResponse<bool>> ChangePassword(ChangePasswordDto dto)
        {
            return await _service.ChangePasswordAsync(dto, _currentUser.UserId);
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<AdminListItemDto>> GetAdminById(int id)
        {
            return await _service.GetAdminByIdAsync(id);
        }
    }
}
