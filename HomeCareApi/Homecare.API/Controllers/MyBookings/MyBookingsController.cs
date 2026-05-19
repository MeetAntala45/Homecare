using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyBookings;
using Homecare.Application.Interfaces.MyBookings;
using Microsoft.AspNetCore.Mvc;
namespace Homecare.API.Controllers;

[ApiController]
[Route("api/my-bookings")]
public class MyBookingsController : ControllerBase
{
    private readonly IMyBookingsService _myBookingsService;

    public MyBookingsController(IMyBookingsService myBookingsService)
    {
        _myBookingsService = myBookingsService;
    }

    [HttpGet("{customerId}")]
    public async Task<ApiResponse<List<MyBookingResponseDto>>> GetBookingsByCustomerId(int customerId)
    {
        return await _myBookingsService.GetBookingsByCustomerIdAsync(customerId);
    }
}