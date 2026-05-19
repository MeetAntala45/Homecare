using Homecare.Data;
using Homecare.Application.DTOs.Profile;
using Homecare.Application.Constants;
using Homecare.Application.Constants.AdminProfile;
using Homecare.Application.Interfaces.Profile;
using Homecare.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Homecare.Application.Interfaces;

namespace Homecare.Application.Services.Profile
{
    public class AdminProfileService : IAdminProfileService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ICloudinaryService _cloudinary;

        private readonly IPasswordHasher<Admin> _passwordHasher;

        public AdminProfileService(AppDbContext context, ICloudinaryService cloudinary, IWebHostEnvironment env, IPasswordHasher<Admin> passwordHasher)
        {
            _context = context;
            _env = env;
            _cloudinary = cloudinary;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApiResponse<AdminProfileDto>> GetProfileAsync(int adminId)
        {
            var admin = await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == adminId && !a.IsDeleted);

            if (admin == null)
                return ApiResponse<AdminProfileDto>.Fail(AdminProfileMessages.AdminNotFound);

            var dto = new AdminProfileDto
            {
                Name = admin.Name,
                Role = admin.Role.ToString(),
                MobileNumber = admin.MobileNumber ?? string.Empty,
                Email = admin.Email,
                Address = admin.Address ?? string.Empty,
                ProfileImage = admin.ProfileImage ?? string.Empty
            };

            return ApiResponse<AdminProfileDto>.SuccessResponse(AdminProfileMessages.ProfileFetched, dto);
        }

        public async Task<ApiResponse<string>> UpdateContactInfoAsync(int adminId, UpdateContactDto dto)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null)
                return ApiResponse<string>.Fail(AdminProfileMessages.AdminNotFound);

            var emailExists = await _context.Admins
                .AnyAsync(a => a.Email == dto.Email && a.Id != adminId);
            if (emailExists)
                return ApiResponse<string>.Fail(AdminProfileMessages.EmailAlreadyInUse);

            admin.MobileNumber = dto.MobileNumber;
            admin.Email = dto.Email;
            admin.Address = dto.Address;
            admin.ModifiedAt = DateTime.UtcNow;
            admin.ModifiedBy = adminId;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(AdminProfileMessages.ContactUpdated);
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(int adminId, ChangePasswordDto dto)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null)
                return ApiResponse<string>.Fail(AdminProfileMessages.AdminNotFound);

            var result = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, dto.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
                return ApiResponse<string>.Fail(AdminProfileMessages.CurrentPasswordIncorrect);

            admin.PasswordHash = _passwordHasher.HashPassword(admin, dto.NewPassword);
            admin.ModifiedAt = DateTime.UtcNow;
            admin.ModifiedBy = adminId;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(AdminProfileMessages.PasswordChanged);
        }

        public async Task<ApiResponse<string>> UploadPhotoAsync(int adminId, IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return ApiResponse<string>.Fail(AdminProfileMessages.NoFileProvided);

            if (!AdminProfileMessages.AllowedPhotoTypes.Contains(photo.ContentType.ToLower()))
                return ApiResponse<string>.Fail(AdminProfileMessages.InvalidFileType);

            if (photo.Length > 2 * 1024 * 1024)
                return ApiResponse<string>.Fail(AdminProfileMessages.FileTooLarge);

            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null)
                return ApiResponse<string>.Fail(AdminProfileMessages.AdminNotFound);

            // Delete old photo from Cloudinary
            if (!string.IsNullOrEmpty(admin.ProfileImage))
            {
                var oldPublicId = _cloudinary.ExtractPublicId(admin.ProfileImage);
                if (oldPublicId != null)
                    await _cloudinary.DeleteAsync(oldPublicId);
            }

            var photoUrl = await _cloudinary.UploadImageAsync(photo, "admin-profiles");
            admin.ProfileImage = photoUrl;
            admin.ModifiedAt = DateTime.UtcNow;
            admin.ModifiedBy = adminId;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(AdminProfileMessages.PhotoUploaded, photoUrl);
        }
    }
}