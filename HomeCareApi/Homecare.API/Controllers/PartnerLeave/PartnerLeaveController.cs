using System.Security.Claims;
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Application.Interfaces.PartnerLeave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers;

[ApiController]
[Route("api/partner/leaves")]
[Authorize(Roles = "ServicePartner")]
public class PartnerLeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public PartnerLeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    private int PartnerId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new UnauthorizedAccessException("Invalid token");

            return int.Parse(claim);
        }
    }

    //  Apply Leave
    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveRequestDto dto)
    {
        var result = await _leaveService.ApplyLeaveAsync(PartnerId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    //  Get My Leaves
    [HttpGet]
    public async Task<IActionResult> GetMyLeaves([FromQuery] LeaveFilterDto filter)
    {
        var result = await _leaveService.GetMyLeavesAsync(PartnerId, filter);
        return Ok(result);
    }

    //  Cancel Leave
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _leaveService.CancelLeaveAsync(id, PartnerId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}