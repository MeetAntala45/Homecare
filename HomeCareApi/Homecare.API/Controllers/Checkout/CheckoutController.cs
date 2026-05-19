using Homecare.Application.Constants;
using Homecare.Application.DTOs.Checkout;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Checkout
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ICurrentUserService _currentUser;

        public CheckoutController(
            ICheckoutService checkoutService,
            ICurrentUserService currentUser)
        {
            _checkoutService = checkoutService;
            _currentUser = currentUser;
        }

        [HttpGet("coupons")]
        public async Task<ApiResponse<List<AvailableCouponDto>>> GetAvailableCoupons(
            [FromQuery] AvailableCouponRequestDto dto)
        {
            return await _checkoutService.GetAvailableCouponsAsync(
                dto, _currentUser.UserId);
        }

        [HttpPost("apply-coupon")]
        public async Task<ApiResponse<ApplyCouponResponseDto>> ApplyCoupon(
            [FromBody] ApplyCouponRequestDto dto)
        {
            return await _checkoutService.ApplyCouponAsync(dto, _currentUser.UserId);
        }

        [AllowAnonymous]
        [HttpPost("summary")]
        public async Task<ApiResponse<CheckoutSummaryResponseDto>> GetSummary(
            [FromBody] CheckoutSummaryRequestDto dto)
        {
            return await _checkoutService.GetSummaryAsync(dto, _currentUser.UserId);
        }
    }
}
