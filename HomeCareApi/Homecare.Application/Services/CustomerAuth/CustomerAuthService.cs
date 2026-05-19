using System.Security.Cryptography;
using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.CustomerAuth;
using Homecare.Application.DTOs.CustomerAuth;
using Homecare.Application.Interfaces.Auth;
using Homecare.Application.Interfaces.CustomerAuth;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.CustomerAuth;

public class CustomerAuthService : ICustomerAuthService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITokensService _tokenService;
    private readonly IWebHostEnvironment _env;
    private readonly EmailTemplateService _templateService;


    public CustomerAuthService(
    AppDbContext context,
    IEmailService emailService,
    ITokensService tokenService,
    IWebHostEnvironment env,
    EmailTemplateService templateService)
    {
        _context = context;
        _emailService = emailService;
        _tokenService = tokenService;
        _env = env;
        _templateService = templateService;
    }

    public async Task<ApiResponse<SendOtpResponse>> SendOtpAsync(SendOtpRequest request)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == request.Email);
        if (customer != null && customer.Status == UserStatus.Blocked)
            return ApiResponse<SendOtpResponse>.Fail(CustomerAuthMessages.Blocked);

        if (customer != null && customer.Status == UserStatus.Inactive)
            return ApiResponse<SendOtpResponse>.Fail(CustomerAuthMessages.Deleted);

        var lastOtp = await _context.OtpVerifications
            .Where(o => o.Email.ToLower() == request.Email.ToLower())
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastOtp != null)
        {
            var minutesSinceLastOtp = (DateTime.UtcNow - lastOtp.CreatedAt).TotalMinutes;

            if (minutesSinceLastOtp < 30)
            {
                var recentOtpCount = await _context.OtpVerifications
                    .Where(o => o.Email.ToLower() == request.Email.ToLower()
                             && o.CreatedAt >= lastOtp.CreatedAt.AddMinutes(-10))
                    .CountAsync();

                if (recentOtpCount >= 5)
                {
                    var canRetryAt = lastOtp.CreatedAt.AddMinutes(30);
                    var secondsRemaining = (int)(canRetryAt - DateTime.UtcNow).TotalSeconds;

                    if (secondsRemaining > 0)
                    {
                        return ApiResponse<SendOtpResponse>.Fail(
                            CustomerAuthMessages.OtpRateLimited,
                            new SendOtpResponse
                            {
                                CooldownSeconds = secondsRemaining,
                                IsRateLimited = true
                            }
                        );
                    }
                }
            }
        }

        if (lastOtp != null)
        {
            var secondsSinceLastOtp = (DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds;
            if (secondsSinceLastOtp < 60)
            {
                var secondsRemaining = (int)(60 - secondsSinceLastOtp);
                return ApiResponse<SendOtpResponse>.Fail(
                    CustomerAuthMessages.OtpCooldown(secondsRemaining),
                    new SendOtpResponse
                    {
                        CooldownSeconds = secondsRemaining,
                        IsRateLimited = false
                    }
                );
            }
        }

        var previousOtps = await _context.OtpVerifications
            .Where(o => o.Email.ToLower() == request.Email.ToLower()
                     && !o.IsUsed
                     && !o.IsRevoked)
            .ToListAsync();

        foreach (var otp in previousOtps)
            otp.IsRevoked = true;

        var otpCode = GenerateOtp();

        var newOtp = new OtpVerification
        {
            Email = request.Email.ToLower(),
            OtpCode = otpCode,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _context.OtpVerifications.Add(newOtp);
        await _context.SaveChangesAsync();

        var htmlBody = _templateService.GetTemplate(
            "OtpEmailTemplate.html",
            new Dictionary<string, string>
            {
                { "Name", request.Name ?? "User" },
                { "OtpCode", otpCode }
            }
        );

        await _emailService.SendAsync(
            request.Email,
            CustomerAuthMessages.OtpSubject,
            htmlBody
        );

        return ApiResponse<SendOtpResponse>.SuccessResponse(
            CustomerAuthMessages.OtpSent,
            new SendOtpResponse
            {
                CooldownSeconds = 60,
                IsRateLimited = false
            }
        );
    }

    private static string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(0, 9999).ToString("D4");
    }

    public async Task<ApiResponse<VerifyOtpResponse>> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var otp = await _context.OtpVerifications
            .FirstOrDefaultAsync(o => o.Email.ToLower() == request.Email.ToLower()
                                && o.OtpCode == request.OtpCode
                                && !o.IsUsed
                                && !o.IsRevoked);

        if (otp == null)
            return ApiResponse<VerifyOtpResponse>.Fail(CustomerAuthMessages.InvalidOtp);

        if (otp.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<VerifyOtpResponse>.Fail(CustomerAuthMessages.OtpExpired);

        otp.IsUsed = true;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == request.Email.ToLower());

        bool isNewUser;

        if (customer == null)
        {
            customer = new Customer
            {
                Name = request.Name.ToTitleCase(),
                Email = request.Email.ToLower(),
                CreatedAt = DateTime.UtcNow,
                Status = UserStatus.Active,
                CreatedBy = null
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            isNewUser = true;
        }
        else
        {
            isNewUser = false;

            if (customer.Name != request.Name)
            {
                customer.Name = request.Name.ToTitleCase();
                customer.ModifiedAt = DateTime.UtcNow;
                customer.ModifiedBy = null;
            }
        }

        otp.CustomerId = customer.Id;

        var previousTokens = await _context.RefreshTokens
            .Where(r => r.CustomerId == customer.Id && r.AdminId == null  && !r.IsRevoked)
            .ToListAsync();

        foreach (var token in previousTokens)
            token.IsRevoked = true;

        var accessToken = _tokenService.GenerateAccessToken(customer.Id, customer.Email, "Customer");
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            CustomerId = customer.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            AdminId = null
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return ApiResponse<VerifyOtpResponse>.SuccessResponse(
            isNewUser ? CustomerAuthMessages.SignUpSuccess : CustomerAuthMessages.LoginSuccess,
            new VerifyOtpResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiry = refreshToken.ExpiresAt,
                IsNewUser = isNewUser
            }
        );
    }

    public async Task<ApiResponse<VerifyOtpResponse>> RefreshTokenAsync(string refreshToken)
    {
        var _refreshToken = await _context.RefreshTokens
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Token == refreshToken
                                    && r.CustomerId != null
                                    && r.AdminId == null);

        if (_refreshToken == null || _refreshToken.IsRevoked)
            return ApiResponse<VerifyOtpResponse>.Fail(CustomerAuthMessages.InvalidRefreshToken);

        if (_refreshToken.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<VerifyOtpResponse>.Fail(CustomerAuthMessages.RefreshTokenExpired);

        var customer = _refreshToken.Customer!;

        var newAccessToken = _tokenService.GenerateAccessToken(customer.Id, customer.Email, "Customer");
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        _refreshToken.Token = newRefreshTokenValue;
        _refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);

        var otherTokens = await _context.RefreshTokens
        .Where(r => r.CustomerId == customer.Id
                 && r.AdminId == null
                 && r.Id != _refreshToken.Id
                 && !r.IsRevoked)
        .ToListAsync();

        foreach (var token in otherTokens)
            token.IsRevoked = true;

         await _context.SaveChangesAsync();

        return ApiResponse<VerifyOtpResponse>.SuccessResponse(
            CustomerAuthMessages.TokenRefreshed,
            new VerifyOtpResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                RefreshTokenExpiry = _refreshToken.ExpiresAt,
                IsNewUser = false
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

        return ApiResponse<string>.SuccessResponse(CustomerAuthMessages.LogoutSuccess);
    }
}