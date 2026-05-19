
using Homecare.Application.Constants;
using Homecare.Application.DTOs.Checkout;

namespace Homecare.Application.Interfaces.Checkout;

public interface ICheckoutService
{
    Task<ApiResponse<List<AvailableCouponDto>>> GetAvailableCouponsAsync(AvailableCouponRequestDto dto, int customerId);

    Task<ApiResponse<ApplyCouponResponseDto>> ApplyCouponAsync(ApplyCouponRequestDto dto, int customerId);

    Task<ApiResponse<CheckoutSummaryResponseDto>> GetSummaryAsync(CheckoutSummaryRequestDto dto, int customerId);
}
