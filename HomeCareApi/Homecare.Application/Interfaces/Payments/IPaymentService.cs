using Homecare.Application.Constants;
using Homecare.Application.DTOs.Payments;
using Microsoft.AspNetCore.Http;

namespace Homecare.Application.Interfaces.Payments;

public interface IPaymentService
{
    Task<ApiResponse<CheckoutSessionResponseDto>> CreateCheckoutSessionAsync(int bookingId);
    Task<ApiResponse<bool>> HandleWebhookAsync(string json, string stripeSignature);
    Task<ApiResponse<bool>> ExpireTimedOutBookingsAsync();
    Task<ApiResponse<bool>> AutoCompleteBookingsAsync(); 
    Task<ApiResponse<bool>> AutoStartBookingsAsync();
    Task<ApiResponse<BookingSuccessResponseDto>> GetBookingSuccessDetailsAsync(int bookingId, int UserId);
    Task<ApiResponse<bool>> RefundBookingAsync(int bookingId);

}