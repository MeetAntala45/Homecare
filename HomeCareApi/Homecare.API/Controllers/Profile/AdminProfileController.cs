using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Homecare.Application.DTOs.Profile;
using Homecare.Application.Interfaces.Profile;
using Homecare.Application.Constants;
using Homecare.Application.Constants.AdminProfile;
using Homecare.Application.Interfaces;

namespace HomeCare.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/profile")]
    public class AdminProfileController : ControllerBase
    {
        private readonly IAdminProfileService _profileService;
        private readonly ICurrentUserService _currentUser;

        public AdminProfileController(IAdminProfileService profileService,
        ICurrentUserService currentUser)
        {
            _profileService = profileService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ApiResponse<AdminProfileDto>> GetProfile()
        {
            var adminId = _currentUser.UserId;
            if (adminId == 0) 
                return ApiResponse<AdminProfileDto>.Fail(AdminProfileMessages.Unauthorized);

            return await _profileService.GetProfileAsync(adminId);
        }

        [HttpPut("contact")]
        public async Task<ApiResponse<string>> UpdateContact([FromBody] UpdateContactDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(AdminProfileMessages.InvalidRequest);

            var adminId = _currentUser.UserId;
            if (adminId == 0) 
                return ApiResponse<string>.Fail(AdminProfileMessages.Unauthorized);

            return await _profileService.UpdateContactInfoAsync(adminId, dto);
        }

        [HttpPut("change-password")]
        public async Task<ApiResponse<string>> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(AdminProfileMessages.InvalidRequest);

            var adminId = _currentUser.UserId;
            if (adminId == 0) 
                return ApiResponse<string>.Fail(AdminProfileMessages.Unauthorized);

            return await _profileService.ChangePasswordAsync(adminId, dto);
        }

        [HttpPost("photo")]
        public async Task<ApiResponse<string>> UploadPhoto(IFormFile photo)
        {
            var adminId = _currentUser.UserId;
            if (adminId == 0) 
                return ApiResponse<string>.Fail(AdminProfileMessages.Unauthorized);

            return await _profileService.UploadPhotoAsync(adminId, photo);
        }
    }
}