using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Homecare.Application.DTOs.Auth;
using Homecare.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Homecare.Application.Constants;
using Homecare.Application.Constants.Auth;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Homecare.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokensService _tokenService;
    private readonly IPasswordHasher<Admin> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly EmailTemplateService _templateService;

    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        ITokensService tokenService,
        IPasswordHasher<Admin> passwordHasher,
        IEmailService emailService,
        IConfiguration config,
        EmailTemplateService templateService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _config = config;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (admin == null)
            return ApiResponse<LoginResponseDto>.Fail(AuthMessages.InvalidCredentials);

        var result = _passwordHasher.VerifyHashedPassword(
            admin,
            admin.PasswordHash,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
            return ApiResponse<LoginResponseDto>.Fail(AuthMessages.InvalidCredentials);

        var existingTokens = await _context.RefreshTokens
            .Where(r => r.AdminId == admin.Id && r.CustomerId == null && !r.IsRevoked)
            .ToListAsync();

        foreach (var token in existingTokens)
            token.IsRevoked = true;

        var accessToken = _tokenService.GenerateAccessToken(admin.Id, admin.Email, admin.Role.ToString());
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        _logger.LogInformation("Admin logged in. Email: {Email} | Role: {Role}", admin.Email, admin.Role);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            AdminId = admin.Id
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return ApiResponse<LoginResponseDto>.SuccessResponse(
            AuthMessages.LoginSuccess,
            new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiry = refreshToken.ExpiresAt,
                Role = admin.Role
            }
        );
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshTokenValue)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.Admin)
            .FirstOrDefaultAsync(r => r.Token == refreshTokenValue && r.AdminId != null);

        if (refreshToken == null ||
            refreshToken.IsRevoked ||
            refreshToken.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<LoginResponseDto>.Fail(AuthMessages.InvalidRefreshToken);

        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        refreshToken.Token = newRefreshTokenValue;
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);

        var otherTokens = await _context.RefreshTokens
        .Where(r => r.AdminId == refreshToken.AdminId &&
                    r.Id != refreshToken.Id &&
                    r.CustomerId == null  &&
                    !r.IsRevoked)
        .ToListAsync();

        foreach (var token in otherTokens)
            token.IsRevoked = true;

        var newAccessToken = _tokenService.GenerateAccessToken(
            refreshToken.Admin!.Id,
            refreshToken.Admin!.Email,
            refreshToken.Admin!.Role.ToString()!
        );

        await _context.SaveChangesAsync();

        return ApiResponse<LoginResponseDto>.SuccessResponse(
            AuthMessages.TokenRefreshed,
            new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                RefreshTokenExpiry = refreshToken.ExpiresAt
            }
        );
    }

    public async Task<ApiResponse<string>> LogoutAsync(string refreshTokenValue)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshTokenValue);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        return ApiResponse<string>.SuccessResponse(AuthMessages.LogoutSuccess);
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (admin == null)
            return ApiResponse<string>.Fail(AuthMessages.EmailNotRegistered);

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hashedToken = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))
        );

        var tokenEntity = new PasswordResetToken
        {
            AdminId = admin.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };

        _context.PasswordResetTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        var resetLink =
            $"{_config["Frontend:BaseUrl"]}/reset-password?token={Uri.EscapeDataString(rawToken)}";

        var htmlBody = _templateService.GetTemplate(
            "ForgotPassword.html",
            new Dictionary<string, string>
            {
                { "ResetLink", resetLink },
            }
        );

        await _emailService.SendAsync(
            request.Email,
            AuthMessages.ForgotPasswordEmailSubject,
            htmlBody
        );

        return ApiResponse<string>.SuccessResponse(AuthMessages.ResetLinkSent);
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return ApiResponse<string>.Fail(AuthMessages.PasswordMismatch);

        var hashedToken = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.Token))
        );

        var tokenEntity = await _context.PasswordResetTokens
            .Include(t => t.Admin)
            .FirstOrDefaultAsync(t => t.Token == hashedToken);

        var validation = ValidateTokenEntity(tokenEntity);
        if (validation != null) return validation;

        tokenEntity!.Admin.PasswordHash = _passwordHasher
            .HashPassword(tokenEntity.Admin, request.NewPassword);

        tokenEntity.IsUsed = true;
        tokenEntity.Admin.ModifiedAt = DateTime.UtcNow;
        tokenEntity.Admin.ModifiedBy = tokenEntity.AdminId;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(AuthMessages.PasswordResetSuccess);
    }

    public async Task<ApiResponse<string>> ValidateResetTokenAsync(string token)
    {
        var hashedToken = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token))
        );

        var tokenEntity = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == hashedToken);

        var validation = ValidateTokenEntity(tokenEntity);
        if (validation != null) return validation;

        return ApiResponse<string>.SuccessResponse(AuthMessages.ResetTokenValid);
    }

    private ApiResponse<string>? ValidateTokenEntity(PasswordResetToken? tokenEntity)
    {
        if (tokenEntity == null)
            return ApiResponse<string>.Fail(AuthMessages.InvalidResetLink);

        if (tokenEntity.IsUsed)
            return ApiResponse<string>.Fail(AuthMessages.ResetLinkAlreadyUsed);

        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<string>.Fail(AuthMessages.ResetLinkExpired);

        return null;
    }
}