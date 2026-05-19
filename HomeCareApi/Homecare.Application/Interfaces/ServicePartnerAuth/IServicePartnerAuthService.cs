using Homecare.Application.Constants;
using Homecare.Application.DTOs.CustomerAuth;
using Homecare.Application.DTOs.ServicePartnerAuth;

namespace Homecare.Application.Interfaces.ServicePartnerLogin;

public interface IServicePartnerAuthService
{
    Task<ApiResponse<SendOtpResponse>> SendOtpAsync(PartnerOtpRequest req);

    Task<ApiResponse<VerifyOtpResponse>> VerifyOtpAsync(VerifyPartnerOtpRequest req);

    Task<ApiResponse<VerifyOtpResponse>> RefreshTokenAsync(string refreshToken);

    Task<ApiResponse<string>> LogoutAsync(string refreshToken);

}
