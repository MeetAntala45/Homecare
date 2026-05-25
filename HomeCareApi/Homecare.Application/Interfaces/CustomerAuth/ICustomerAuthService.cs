using Homecare.Application.Constants;
using Homecare.Application.DTOs.CustomerAuth;

namespace Homecare.Application.Interfaces.CustomerAuth;

public interface ICustomerAuthService
{
    Task<ApiResponse<SendOtpResponse>> SendOtpAsync(SendOtpRequest request);
    Task<ApiResponse<VerifyOtpResponse>> VerifyOtpAsync(VerifyOtpRequest request);
    Task<ApiResponse<VerifyOtpResponse>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<string>> LogoutAsync(string refreshTokenValue);
    Task<ApiResponse<ValidateReferralCodeResponse>> ValidateReferralCodeAsync(
    ValidateReferralCodeRequest request);
}
