using System.Data;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.BookingManagement;
using Homecare.Application.DTOs.Bookings;
using Homecare.Application.Interfaces.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Bookings;

[ApiController]
[Route("api/admin/bookings")]
[Authorize]
public class BookingGridController : ControllerBase
{
    private readonly IBookingGridService _bookingGridService;

    public BookingGridController(IBookingGridService bookingGridService)
    {
        _bookingGridService = bookingGridService;
    }

    [HttpGet("customer-summaries")]
    public async Task<ApiResponse<CustomerBookingSummaryPagedDto>> GetCustomerBookingSummaries(
        [FromQuery] BookingGridRequestDto request)
    {
        return await _bookingGridService.GetCustomerBookingSummariesAsync(request);
    }

    [HttpDelete("customers/{customerId:int}")]
    public async Task<ApiResponse<bool>> DeleteCustomerBookings(int customerId, [FromQuery] int paymentMethodValue)
    {
        return await _bookingGridService.DeleteCustomerBookingsAsync(customerId, paymentMethodValue);
    }

    [HttpGet("customers/{customerId}/details")]
    public async Task<IActionResult> GetCustomerBookingDetails(
    int customerId,
    [FromQuery] CustomerBookingDetailRequestDto request)
    {
        request.CustomerId = customerId;

        var result = await _bookingGridService.GetCustomerBookingDetailsAsync(request);

        return Ok(result);
    }

    [HttpGet("service-types")]
    public async Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypes()
    {
        return await _bookingGridService.GetServiceTypesAsync();
    }

    [HttpGet("page-of/{customerId}/{paymentMethod}")]
    public async Task<IActionResult> GetCustomerPage(int customerId, int paymentMethod,
    [FromQuery] int pageSize = 10)
    {
        var position = await _bookingGridService.GetCustomerPositionAsync(customerId, paymentMethod, pageSize);
        return Ok(ApiResponse<int>.SuccessResponse("Page found", position));
    }
}