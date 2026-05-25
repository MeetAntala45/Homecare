using Homecare.Application.Constants;
using Homecare.Application.Constants.CustomerAuth;
using Homecare.Application.DTOs.CustomerAuth;
using Homecare.Application.Interfaces.CustomerAuth;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.CustomerAuth
{
    [Route("api/customer-auth")]
    [ApiController]
    public class CustomerAuthController : ControllerBase
    {
        private readonly ICustomerAuthService _customerAuthService;

        public CustomerAuthController(ICustomerAuthService customerAuthService)
        {
            _customerAuthService = customerAuthService;
        }

        [HttpPost("send-otp")]
        public async Task<ApiResponse<SendOtpResponse>> SendOtp(SendOtpRequest req)
        {
            var result = await _customerAuthService.SendOtpAsync(req);
            return result;
        }

        [HttpPost("verify-otp")]
        public async Task<ApiResponse<VerifyOtpResponse>> VerifyOtp(VerifyOtpRequest req)
        {
            var result = await _customerAuthService.VerifyOtpAsync(req);

            if (result.Success && result.Data != null)
            {
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
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
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return ApiResponse<VerifyOtpResponse>.Fail(CustomerAuthMessages.RefreshTokenMissing);

            var result = await _customerAuthService.RefreshTokenAsync(refreshToken);

            if (result.Success && result.Data != null)
            {
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
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
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await _customerAuthService.LogoutAsync(refreshToken!);

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return result;
        }

        [HttpPost("validate-referral-code")]
        public async Task<ApiResponse<ValidateReferralCodeResponse>> ValidateReferralCode(
        [FromBody] ValidateReferralCodeRequest req)
        {
            return await _customerAuthService.ValidateReferralCodeAsync(req);
        }
    }


}