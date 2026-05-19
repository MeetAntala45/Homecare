using Homecare.Application.DTOs.Auth;
using Homecare.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;
using Homecare.Application.Constants;

namespace Homecare.API.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private const string RefreshTokenCookie = "admin_refreshToken";

    [HttpPost("login")]
    public async Task<ApiResponse<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.Success && result.Data != null)
        {
            Response.Cookies.Append(RefreshTokenCookie, result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.RefreshTokenExpiry
            });
        }

        return result;
    }

    [HttpPost("refresh")]
    public async Task<ApiResponse<LoginResponseDto>> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (result.Success && result.Data != null)
        {
            Response.Cookies.Append(RefreshTokenCookie, result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.RefreshTokenExpiry
            });

            result.Data.RefreshToken = null!;
        }

        return result;
    }

    [HttpPost("logout")]
    public async Task<ApiResponse<string>> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];

        var result = await _authService.LogoutAsync(refreshToken);

        Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });

        return result;
    }

    [HttpPost("reset-password")]
    public async Task<ApiResponse<string>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result;
    }

    [HttpGet("validate-reset-token")]
    public async Task<ApiResponse<string>> ValidateResetToken([FromQuery] string token)
    {
        var result = await _authService.ValidateResetTokenAsync(token);
        return result;
    }

    [HttpPost("forgot-password")]
    public async Task<ApiResponse<string>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return result;
    }
}