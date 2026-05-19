using Homecare.Application.Constants;
using Homecare.Application.DTOs.Tracking;
using Homecare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Tracking;

[ApiController]
[Route("api/tracking")]
public class TrackingController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;

    public TrackingController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpPost("update-location")]
    [Authorize(Roles = "ServicePartner")]
    public async Task<ApiResponse<string>> UpdateLocation([FromBody] UpdateLocationDto dto)
    {
        return ApiResponse<string>.SuccessResponse("Location tracking coming soon.", null);
    }

    [HttpGet("last-location/{bookingId:int}")]
    [Authorize(Roles = "Customer")]
    public async Task<ApiResponse<LocationResponseDto>> GetLastLocation(int bookingId)
    {
        return ApiResponse<LocationResponseDto>.SuccessResponse("Location tracking coming soon..", null);
    }

}
