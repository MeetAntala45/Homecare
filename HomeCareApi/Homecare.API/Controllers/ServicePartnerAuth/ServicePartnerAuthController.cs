using Homecare.Application.Constants;
using Homecare.Application.DTOs.CustomerAuth;
using Homecare.Application.DTOs.ServicePartnerAuth;
using Homecare.Application.Interfaces.ServicePartnerLogin;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.ServicePartnerAuth
{
    [Route("api/service-partner-auth")]
    [ApiController]
    public class ServicePartnerAuthController : ControllerBase
    {

        private readonly IServicePartnerAuthService _service;

        public ServicePartnerAuthController(IServicePartnerAuthService service)
        {
            _service = service;
        }

        [HttpPost("send-otp")]
        public async Task<ApiResponse<SendOtpResponse>> SendOtp(PartnerOtpRequest req)
        {
            return await _service.SendOtpAsync(req);
        }

        [HttpPost("verify-otp")]
        public async Task<ApiResponse<VerifyOtpResponse>> VerifyOtp(VerifyPartnerOtpRequest req)
        {
            var result = await _service.VerifyOtpAsync(req);

            if (result.Success && result.Data != null)
            {
                Response.Cookies.Append("partnerRefreshToken", result.Data.RefreshToken, new CookieOptions
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
        public async Task<ApiResponse<VerifyOtpResponse>> Refresh()
        {
            var refreshToken = Request.Cookies["partnerRefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return ApiResponse<VerifyOtpResponse>.Fail("Refresh token missing");

            var result = await _service.RefreshTokenAsync(refreshToken);

            if (result.Success && result.Data != null)
            {
                Response.Cookies.Append("partnerRefreshToken", result.Data.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = result.Data.RefreshTokenExpiry
                });
            }

            return result;
        }

        [HttpPost("logout")]
        public async Task<ApiResponse<string>> Logout()
        {
            var refreshToken = Request.Cookies["partnerRefreshToken"];

            var result = await _service.LogoutAsync(refreshToken!);

            Response.Cookies.Delete("partnerRefreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return result;
        }
    }
}