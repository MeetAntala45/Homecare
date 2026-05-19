using Homecare.Application.DTOs.Profile;
using Homecare.Application.Constants;
using Microsoft.AspNetCore.Http;

namespace Homecare.Application.Interfaces.Profile
{
    public interface IAdminProfileService
    {
        Task<ApiResponse<AdminProfileDto>> GetProfileAsync(int adminId);
        Task<ApiResponse<string>> UpdateContactInfoAsync(int adminId, UpdateContactDto dto);
        Task<ApiResponse<string>> ChangePasswordAsync(int adminId, ChangePasswordDto dto);
        Task<ApiResponse<string>> UploadPhotoAsync(int adminId, IFormFile photo);
    }
}