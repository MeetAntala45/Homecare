using Homecare.Application.DTOs.Auth;
using Homecare.Application.Constants;

namespace Homecare.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<string>> LogoutAsync(string refreshTokenValue);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<string>> ValidateResetTokenAsync(string token);
}