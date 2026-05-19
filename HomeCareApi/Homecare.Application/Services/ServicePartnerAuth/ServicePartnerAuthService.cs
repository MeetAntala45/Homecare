using System.Security.Cryptography;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.CustomerAuth;
using Homecare.Application.DTOs.ServicePartnerAuth;
using Homecare.Application.Interfaces.Auth;
using Homecare.Application.Interfaces.ServicePartnerLogin;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.ServicePartnerLogin;

public class ServicePartnerAuthService : IServicePartnerAuthService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITokensService _tokenService;
    private readonly EmailTemplateService _templateService;
    public ServicePartnerAuthService(
        AppDbContext context,
        IEmailService emailService,
        ITokensService tokenService,
        EmailTemplateService templateService
        )
    {
        _emailService = emailService;
        _tokenService = tokenService;
        _templateService = templateService;
        _context = context;
    }

    public async Task<ApiResponse<SendOtpResponse>> SendOtpAsync(PartnerOtpRequest req)
    {

        if (string.IsNullOrWhiteSpace(req.Email))
            return ApiResponse<SendOtpResponse>.Fail("Email is required");

        var email = req.Email.ToLower();

        var partner = await _context.ServicePartners
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email && !x.IsDeleted);

        if (partner == null)
            return ApiResponse<SendOtpResponse>.Fail("Service partner not found");

        if (partner.Status == PartnerStatus.Pending)
            return ApiResponse<SendOtpResponse>.Fail("Your account is not approved yet");

        if (partner.Status == PartnerStatus.Rejected)
            return ApiResponse<SendOtpResponse>.Fail("Your account approval is rejected");

        var lastOtp = await _context.PartnerOtpVerifications
            .Where(o => o.Email.ToLower() == email)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastOtp != null)
        {
            var minutesSinceLastOtp = (DateTime.UtcNow - lastOtp.CreatedAt).TotalMinutes;

            if (minutesSinceLastOtp < 30)
            {
                var recentOtpCount = await _context.PartnerOtpVerifications
                    .Where(o => o.Email.ToLower() == email
                             && o.CreatedAt >= lastOtp.CreatedAt.AddMinutes(-10))
                    .CountAsync();

                if (recentOtpCount >= 5)
                {
                    var canRetryAt = lastOtp.CreatedAt.AddMinutes(30);
                    var secondsRemaining = (int)(canRetryAt - DateTime.UtcNow).TotalSeconds;

                    if (secondsRemaining > 0)
                    {
                        return ApiResponse<SendOtpResponse>.Fail(
                            "Too many OTP requests. Please try again after 30 minutes.",
                            new SendOtpResponse
                            {
                                CooldownSeconds = secondsRemaining,
                                IsRateLimited = true
                            }
                        );
                    }
                }
            }

            var secondsSinceLastOtp = (DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds;

            if (secondsSinceLastOtp < 60)
            {
                var secondsRemaining = (int)(60 - secondsSinceLastOtp);

                return ApiResponse<SendOtpResponse>.Fail(
                    $"Please wait {secondsRemaining} seconds before requesting a new OTP.",
                    new SendOtpResponse
                    {
                        CooldownSeconds = secondsRemaining,
                        IsRateLimited = false
                    }
                );
            }
        }

        var existingOtps = await _context.PartnerOtpVerifications
            .Where(x => x.Email.ToLower() == email && !x.IsUsed)
            .ToListAsync();

        foreach (var item in existingOtps)
            item.IsRevoked = true;

        var otp = GenerateOtp();

        var otpEntity = new PartnerOtpVerification
        {
            ServicePartnerId = partner.Id,
            Email = email,
            OtpCode = otp,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _context.PartnerOtpVerifications.Add(otpEntity);
        await _context.SaveChangesAsync();

        var emailBody = _templateService.GetTemplate("OtpEmailTemplate.html",
            new Dictionary<string, string>
            {
                { "Name", partner.FullName ?? "User" },
                { "OtpCode", otp }
            }
        );

        await _emailService.SendAsync(email, "Your OTP Code", emailBody);

        return ApiResponse<SendOtpResponse>.SuccessResponse("Otp sent successfully", new SendOtpResponse
        {
            CooldownSeconds = 60,
            IsRateLimited = false
        });
    }

    public async Task<ApiResponse<VerifyOtpResponse>> VerifyOtpAsync(VerifyPartnerOtpRequest req)
    {
        var email = req.Email.ToLower();

        var otp = await _context.PartnerOtpVerifications
            .Where(x =>
                x.Email.ToLower() == email &&
                x.OtpCode == req.OtpCode &&
                !x.IsUsed &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
            return ApiResponse<VerifyOtpResponse>.Fail("Invalid or expired OTP");

        var partner = await _context.ServicePartners
            .FirstOrDefaultAsync(x => x.Id == otp.ServicePartnerId);

        if (partner == null)
            return ApiResponse<VerifyOtpResponse>.Fail("Service partner not found");

        otp.IsUsed = true;

        var oldTokens = await _context.RefreshTokens
            .Where(x => x.ServicePartnerId == partner.Id && !x.IsRevoked)
            .ToListAsync();

        foreach (var t in oldTokens)
            t.IsRevoked = true;

        var accessToken = _tokenService.GenerateAccessToken(
            partner.Id,
            partner.Email,
            "ServicePartner"
        );

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ServicePartnerId = partner.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return ApiResponse<VerifyOtpResponse>.SuccessResponse("Otp Verified Successfully", new VerifyOtpResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiry = refreshToken.ExpiresAt
        });
    }

    public async Task<ApiResponse<VerifyOtpResponse>> RefreshTokenAsync(string refreshToken)
    {
        var refreshTokenEntity = await _context.RefreshTokens
        .Include(r => r.ServicePartner)
        .FirstOrDefaultAsync(r => r.Token == refreshToken
                                && r.ServicePartnerId != null);

        if (refreshTokenEntity == null || refreshTokenEntity.IsRevoked)
            return ApiResponse<VerifyOtpResponse>.Fail("Invalid refresh token.");

        if (refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<VerifyOtpResponse>.Fail("Refresh token expired.");

        var partner = refreshTokenEntity.ServicePartner!;

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        refreshTokenEntity.Token = newRefreshToken;
        refreshTokenEntity.ExpiresAt = DateTime.UtcNow.AddDays(7);

        var otherTokens = await _context.RefreshTokens
        .Where(r => r.ServicePartnerId == refreshTokenEntity.ServicePartnerId &&
                    r.Id != refreshTokenEntity.Id &&
                    r.CustomerId == null &&
                    !r.IsRevoked)
        .ToListAsync();

        foreach (var token in otherTokens)
            token.IsRevoked = true;

        var newAccessToken = _tokenService.GenerateAccessToken(
            refreshTokenEntity.ServicePartner!.Id,
            refreshTokenEntity.ServicePartner.Email,
            "ServicePartner"
        );

        await _context.SaveChangesAsync();

        return ApiResponse<VerifyOtpResponse>.SuccessResponse(
            "Token refreshed successfully.",
            new VerifyOtpResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiry = refreshTokenEntity.ExpiresAt,
                IsNewUser = false
            }
        );
    }

    public async Task<ApiResponse<string>> LogoutAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        return ApiResponse<string>.SuccessResponse("Logout Successfully");
    }
    private static string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(0, 9999).ToString("D4");
    }
}
