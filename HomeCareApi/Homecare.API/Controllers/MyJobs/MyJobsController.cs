using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.MyJobs;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.MyJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.ServicePartner
{
    [ApiController]
    [Route("api/service-partner")]
    [Authorize]
    public class MyJobsController : ControllerBase
    {
        private readonly IMyJobsService _servicePartnerBookingService;
        private readonly IBookingServicesService _bookingService;

        private readonly ICurrentUserService _currentUser;

        public MyJobsController(IMyJobsService servicePartnerBookingService, ICurrentUserService currentUser, IBookingServicesService bookingService)
        {
            _bookingService = bookingService;
            _servicePartnerBookingService = servicePartnerBookingService;
            _currentUser = currentUser;
        }

        [HttpGet("bookings")]
        public async Task<ApiResponse<PaymentPagedResult<MyJobsDto>>> GetBookingsByPartnerId([FromQuery] MyJobRequestDto req)
        {
            var result = await _servicePartnerBookingService.GetBookingsByPartnerIdAsync(req, _currentUser.UserId);

            return ApiResponse<PaymentPagedResult<MyJobsDto>>
                .SuccessResponse("Partner's jobs fetched successfully", result);
        }

        [HttpGet("bookings/calendar")]
        public async Task<ApiResponse<List<MyJobCalendarDto>>> GetCalendarJobs([FromQuery] MyJobCalendarRequestDto req)
        {
            var result = await _servicePartnerBookingService.GetCalendarJobsAsync(req, _currentUser.UserId);

            return ApiResponse<List<MyJobCalendarDto>>
                .SuccessResponse("Calendar jobs fetched successfully", result);
        }

        [HttpGet("bookings/services")]
        public async Task<IActionResult> GetServiceNames()
        {
            var partnerId = _currentUser.UserId;

            if (partnerId == 0)
                return Unauthorized();

            var services = await _bookingService.GetDistinctServiceNamesAsync(partnerId);

            return Ok(new
            {
                data = services
            });
        }

        [HttpGet("bookings/booking-page/{bookingId}")]
        public async Task<IActionResult> GetBookingPage(int bookingId,
        [FromQuery] int pageSize, [FromQuery] string status)
        {
            var page = await _servicePartnerBookingService.GetBookingPageAsync(
                _currentUser.UserId, bookingId, pageSize, status);
            return Ok(ApiResponse<int>.SuccessResponse("Page found.", page));
        }
    }
}