using Homecare.Application.Constants;
using Homecare.Application.DTOs.CustomerUser;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.CustomerUserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/booking-management")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class BookingManagementController : ControllerBase
{
    private readonly ICustomerManagementService _service;
    private readonly ICurrentUserService _currentUser;

    public BookingManagementController(
        ICustomerManagementService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("{bookingId:int}/available-partners")]
    public async Task<ApiResponse<List<AvailablePartnerDto>>> GetAvailablePartners(int bookingId)
    {
        return await _service.GetAvailablePartnersAsync(bookingId);
    }

    [HttpPatch("{bookingId:int}/change-expert")]
    public async Task<ApiResponse<string>> ChangeExpert(
        int bookingId, [FromBody] ChangeExpertRequestDto dto)
    {
        return await _service.ChangeExpertAsync(bookingId, dto.NewPartnerId, _currentUser.UserId);
    }

    [HttpPatch("{bookingId:int}/complete")]
    public async Task<ApiResponse<string>> CompleteBooking(int bookingId)
    {
        return await _service.CompleteBookingAsync(bookingId, _currentUser.UserId);
    }

    [HttpPatch("{bookingId:int}/cancel")]
    public async Task<ApiResponse<string>> CancelBooking(
        int bookingId, [FromBody] CancelBookingRequestDto dto)
    {
        return await _service.CancelBookingAsync(bookingId, dto.Reason, _currentUser.UserId);
    }

    [HttpDelete("{bookingId:int}/delete")]
    public async Task<ApiResponse<string>> DeleteBooking(int bookingId)
    {
        return await _service.DeleteBookingAsync(bookingId, _currentUser.UserId);
    }
}