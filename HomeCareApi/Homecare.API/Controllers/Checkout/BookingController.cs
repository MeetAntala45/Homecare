using Homecare.Application.Constants;
using Homecare.Application.Constants.Checkout;
using Homecare.Application.DTOs.Checkout;
using Homecare.Application.Interfaces.Checkout;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace Homecare.API.Controllers;

[ApiController]
[Route("api/booking")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }
    private int GetCustomerId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("counts")]
    public async Task<ApiResponse<Dictionary<int, int>>> GetAllServiceBookingCounts()
    {
        try
        {
            return await _bookingService.GetAllServiceBookingCountsAsync();
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<Dictionary<int, int>>.Fail(message);
        }
    }

    [HttpGet("counts/serviceType/{serviceTypeId}")]
    public async Task<ApiResponse<int>> GetAllServiceBookingByServiceType(int serviceTypeId)
    {
        try
        {
            return await _bookingService.GetTotalBookingByServiceTypeAsync(serviceTypeId);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<int>.Fail(message);
        }
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<ApiResponse<CreateBookingResponseDto>> CreateBooking(
           [FromBody] CreateBookingRequestDto dto)
    {
        try
        {
            int customerId = GetCustomerId();
            var result = await _bookingService.CreateBooking(dto, customerId);
            return result;
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<CreateBookingResponseDto>.Fail(message);
        }
    }

    [AllowAnonymous]
    [HttpPost("payment-callback")]
    public async Task<ApiResponse<string>> PaymentCallback(
        [FromBody] PaymentCallbackDto dto)
    {
        await _bookingService.HandlePaymentCallback(dto);

        return ApiResponse<string>
            .SuccessResponse(CheckoutBookingMessages.PaymentSuccess, null!);
    }
}