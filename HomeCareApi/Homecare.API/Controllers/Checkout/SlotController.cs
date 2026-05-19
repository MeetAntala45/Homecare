using Homecare.Application.DTOs.Checkout;
using Homecare.Application.Interfaces.Checkout;
using Homecare.Application.Constants;
using Homecare.Application.Constants.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Homecare.Application.Interfaces;

namespace Homecare.API.Controllers;

[ApiController]
[Route("api/slot")]
public class SlotController : ControllerBase
{
    private readonly ISlotService _service;
    private readonly ICurrentUserService _currentUser;

    public SlotController(ISlotService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> GetSlots([FromQuery] GetSlotsRequestDto dto)
    {
        var result = await _service.GetSlots(dto, _currentUser.UserId);
        return Ok(ApiResponse<List<SlotResponseDto>>.SuccessResponse("Slots fetched successfully", result));
    }

    [Authorize]
    [HttpGet("dates")]
    public async Task<ApiResponse<List<DateAvailabilityDto>>> GetDates(int serviceId)
    {
        var result = await _service.GetAvailableDates(serviceId, _currentUser.UserId);

        return ApiResponse<List<DateAvailabilityDto>>
            .SuccessResponse(CheckoutSlotMessages.SlotsFetched, result);
    }

    [HttpGet("partner-availability")]
    public async Task<IActionResult> GetServicePartnerAvailability()
    {
        var result = await _service.GetServicePartnerAvailability();
        return Ok(ApiResponse<Dictionary<int, bool>>.SuccessResponse("Success", result));
    }
}