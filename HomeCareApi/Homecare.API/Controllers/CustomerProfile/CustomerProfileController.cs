using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Homecare.Application.Constants;
using Homecare.Application.Constants.CustomerProfile;
using Homecare.Application.Interfaces.CustomerProfile;
using System.Security.Claims;
using Homecare.Application.DTOs.CustomerProfile;

namespace HomeCare.API.Controllers
{
    [ApiController]
    [Route("api/customer/profile")]
    [Authorize]
    public class CustomerProfileController : ControllerBase
    {
        private readonly ICustomerProfileService _profileService;

        public CustomerProfileController(ICustomerProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<ApiResponse<CustomerProfileDto>> GetProfile()
        {
            return await _profileService.GetProfileAsync(GetCustomerId());
        }

        [HttpPut("mobile")]
        public async Task<ApiResponse<string>> UpdateMobile([FromBody] UpdateMobileDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(CustomerProfileMessages.InvalidData);

            return await _profileService.UpdateMobileAsync(GetCustomerId(), dto);
        }

        [HttpPost("email/request-change")]
        public async Task<ApiResponse<EmailChangeOtpResponseDto>> RequestEmailChange([FromBody] RequestEmailChangeDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<EmailChangeOtpResponseDto>.Fail(CustomerProfileMessages.InvalidData);

            return await _profileService.RequestEmailChangeAsync(GetCustomerId(), dto);
        }

        [HttpPost("email/verify-otp")]
        public async Task<ApiResponse<string>> VerifyEmailChange([FromBody] VerifyEmailChangeDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(CustomerProfileMessages.InvalidData);

            return await _profileService.VerifyEmailChangeAsync(GetCustomerId(), dto);
        }

        [HttpPost("address")]
        public async Task<ApiResponse<string>> AddAddress([FromBody] AddressRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(CustomerProfileMessages.InvalidData);

            return await _profileService.AddAddressAsync(GetCustomerId(), dto);
        }

        [HttpPut("address/{addressId}")]
        public async Task<ApiResponse<string>> EditAddress(int addressId, [FromBody] AddressRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(CustomerProfileMessages.InvalidData);

            return await _profileService.EditAddressAsync(GetCustomerId(), addressId, dto);
        }

        [HttpDelete("address/{addressId}")]
        public async Task<ApiResponse<string>> DeleteAddress(int addressId)
        {
            return await _profileService.DeleteAddressAsync(GetCustomerId(), addressId);
        }

        [HttpGet("address/labels")]
        public async Task<ApiResponse<List<string>>> GetAddressLabels()
        {
            return await _profileService.GetAddressLabelsAsync(GetCustomerId());
        }

        [HttpPost("address/add-recent-search")]
        public async Task<ApiResponse<string>> AddRecentSearch([FromBody] AddRecentSearchDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse<string>.Fail(CustomerProfileMessages.InvalidData); 
            return await _profileService.AddRecentSearchAsync(GetCustomerId(), dto);
        }

        [HttpGet("address/recent-searches")]
        public async Task<ApiResponse<List<RecentSearchDto>>> GetRecentSearches()
        {
            return await _profileService.GetRecentSearchesAsync(GetCustomerId());
        }

        private int GetCustomerId()
        {
            var claim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier
            );
            return int.TryParse(claim?.Value, out var id) ? id : 0;
        }
    }
}