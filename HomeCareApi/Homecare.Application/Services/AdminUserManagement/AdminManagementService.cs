using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.AdminUserManagement;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.AdminUser;
using Homecare.Application.Interfaces.AdminUserManagement;
using Homecare.Application.Interfaces.Auth;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.AdminUserManagement;

public class AdminManagementService : IAdminManagementService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Admin> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly EmailTemplateService _templateService;


    public AdminManagementService(
        AppDbContext context,
        IPasswordHasher<Admin> passwordHasher,
        IEmailService emailService,
        EmailTemplateService templateService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _templateService = templateService;
    }

    public async Task<ApiResponse<int>> SaveAdminUserAsync(AdminDto dto, int currentUser)
    {
        if (dto.Id.HasValue)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Id == dto.Id.Value && !a.IsDeleted);

            if (admin == null)
                return ApiResponse<int>.Fail(AdminManagementMessages.AdminNotFound);

            if (!string.IsNullOrWhiteSpace(dto.Name))
                admin.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.MobileNumber))
                admin.MobileNumber = dto.MobileNumber;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _context.Admins
                    .AnyAsync(a => a.Email == dto.Email
                                && a.Id != dto.Id.Value
                                && !a.IsDeleted);

                if (emailExists)
                    return ApiResponse<int>.Fail(AdminManagementMessages.EmailAlreadyExists);

                admin.Email = dto.Email;
            }

            admin.ModifiedAt = DateTime.UtcNow;
            admin.ModifiedBy = currentUser;

            await _context.SaveChangesAsync();

            return ApiResponse<int>.SuccessResponse(AdminManagementMessages.AdminUpdated, admin.Id);
        }

        var existing = await _context.Admins
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(a => a.Email == dto.Email);

        if (existing != null && !existing.IsDeleted)
            return ApiResponse<int>.Fail(AdminManagementMessages.AdminAlreadyExists);

        if (existing != null && existing.IsDeleted)
        {
            existing.Name = dto.Name;
            existing.MobileNumber = dto.MobileNumber;
            existing.Role = AdminRole.Admin;
            existing.IsDeleted = false;

            bool passwordChanged = false;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var verifyResult = _passwordHasher.VerifyHashedPassword(
                    existing,
                    existing.PasswordHash,
                    dto.Password
                );

                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    existing.PasswordHash = _passwordHasher.HashPassword(existing, dto.Password);
                    passwordChanged = true;
                }
            }

            existing.ModifiedAt = DateTime.UtcNow;
            existing.ModifiedBy = currentUser;

            await _context.SaveChangesAsync();

            if (passwordChanged)
                await SendAdminCredentialsEmail(existing.Email, dto.Password!);

            return ApiResponse<int>.SuccessResponse(
                passwordChanged
                    ? AdminManagementMessages.AdminRestoredWithEmail
                    : AdminManagementMessages.AdminRestored,
                existing.Id
            );
        }

        var adminEntity = new Admin
        {
            Name = dto.Name,
            Email = dto.Email,
            MobileNumber = dto.MobileNumber,
            Role = AdminRole.Admin,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser
        };

        adminEntity.PasswordHash = _passwordHasher.HashPassword(adminEntity, dto.Password!);

        _context.Admins.Add(adminEntity);
        await _context.SaveChangesAsync();

        await SendAdminCredentialsEmail(adminEntity.Email, dto.Password!);

        return ApiResponse<int>.SuccessResponse(AdminManagementMessages.AdminCreated, adminEntity.Id);
    }

    private async Task SendAdminCredentialsEmail(string email, string password)
    {
        var htmlBody = _templateService.GetTemplate(
            "AdminCredentials.html",
            new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", password }
            }
        );

        await _emailService.SendAsync(
            email,
            AdminManagementMessages.CredentialsEmailSubject,
            htmlBody
        );
    }
    
    private async Task SendPasswordChangedEmail(string email, string newPassword)
    {
        var htmlBody = _templateService.GetTemplate(
            "AdminPasswordChanged.html",
            new Dictionary<string, string>
            {
                { "Email", email },
                { "NewPassword", newPassword }
            }
        );

        await _emailService.SendAsync(
            email,
            AdminManagementMessages.PasswordChangedEmailSubject,
            htmlBody
        );
    }

    public async Task<ApiResponse<bool>> DeleteAdminUserAsync(int id, int currentUser)
    {
        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id);

        if (admin == null)
            return ApiResponse<bool>.Fail(AdminManagementMessages.AdminDoesNotExist);

        if (admin.IsDeleted)
            return ApiResponse<bool>.Fail(AdminManagementMessages.AdminAlreadyDeleted);

        admin.IsDeleted = true;
        admin.ModifiedAt = DateTime.UtcNow;
        admin.ModifiedBy = currentUser;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(AdminManagementMessages.AdminDeleted, true);
    }

    public async Task<ApiResponse<PagedResult<AdminListItemDto>>> GetAdminListAsync(AdminListFilterDto filter)
    {
        filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var query = _context.Admins
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AsQueryable();

        if (filter.Role.HasValue)
            query = query.Where(a => a.Role == filter.Role.Value);

        if (filter.IsActive.HasValue)
            query = filter.IsActive.Value
                ? query.Where(a => !a.IsDeleted)
                : query.Where(a => a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            var normalisedName = filter.UserName
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                normalisedName.All(term =>
                    x.Name
                      .ToLower()
                      .Contains(term)
                )
            );
        }


        bool isDesc = filter.SortOrder?.ToLower() == "desc";

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            query = filter.SortBy?.ToLower() switch
            {
                "id" => isDesc
                    ? query.OrderByDescending(a => a.Id)
                    : query.OrderBy(a => a.Id),
                "name" => isDesc
                    ? query.OrderByDescending(a => a.Name)
                    : query.OrderBy(a => a.Name),

                "email" => isDesc
                    ? query.OrderByDescending(a => a.Email)
                    : query.OrderBy(a => a.Email),

                _ => query.OrderByDescending(a => a.Id)
            };
        }
        else
        {
            query = query.OrderByDescending(a => a.Id);
        }

        var totalCount = await query.CountAsync();

        var admins = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AdminListItemDto
            {
                Id = a.Id,
                Name = a.Name.ToTitleCase(),
                Email = a.Email,
                MobileNumber = a.MobileNumber,
                Role = a.Role.ToString(),
                IsActive = !a.IsDeleted
            })
            .ToListAsync();

        var result = new PagedResult<AdminListItemDto>
        {
            Data = admins,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponse<PagedResult<AdminListItemDto>>
            .SuccessResponse(AdminManagementMessages.AdminsFetched, result);
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto dto, int currentUser)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Id == dto.Id && !a.IsDeleted);

        if (admin == null)
            return ApiResponse<bool>.Fail(AdminManagementMessages.AdminDoesNotExist);

        if (dto.NewPassword != dto.ConfirmPassword)
            return ApiResponse<bool>.Fail(AdminManagementMessages.PasswordMismatch);

        admin.PasswordHash = _passwordHasher.HashPassword(admin, dto.NewPassword);
        admin.ModifiedAt = DateTime.UtcNow;
        admin.ModifiedBy = currentUser;

        await _context.SaveChangesAsync();

        await SendPasswordChangedEmail(admin.Email, dto.NewPassword!);

        return ApiResponse<bool>.SuccessResponse(AdminManagementMessages.PasswordChanged, true);
    }

    public async Task<ApiResponse<AdminListItemDto>> GetAdminByIdAsync(int id)
    {
        var admin = await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (admin == null)
            return ApiResponse<AdminListItemDto>.Fail(AdminManagementMessages.AdminDoesNotExist);

        var result = new AdminListItemDto
        {
            Id = admin.Id,
            Name = admin.Name.ToTitleCase(),
            Email = admin.Email,
            MobileNumber = admin.MobileNumber,
            Role = admin.Role.ToString(),
            IsActive = !admin.IsDeleted
        };

        return ApiResponse<AdminListItemDto>.SuccessResponse(AdminManagementMessages.AdminFetched, result);
    }
}