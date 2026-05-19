using Homecare.Application.Constants;
using Homecare.Application.DTOs.Checkout;

namespace Homecare.Application.Interfaces.Checkout;

public interface IBookingService
{
    Task<ApiResponse<CreateBookingResponseDto>> CreateBooking(CreateBookingRequestDto dto, int customerId);
    Task<ApiResponse<Dictionary<int, int>>> GetAllServiceBookingCountsAsync();
    Task<ApiResponse<int>> GetTotalBookingByServiceTypeAsync(int serviceTypeId);
    Task HandlePaymentCallback(PaymentCallbackDto dto);
}