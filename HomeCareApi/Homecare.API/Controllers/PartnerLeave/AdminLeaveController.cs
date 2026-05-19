
using System.Security.Claims;
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Application.Interfaces.PartnerLeave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers;

[ApiController]
[Route("api/admin/leaves")]
[Authorize(Roles = "Admin, SuperAdmin")]
public class AdminLeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public AdminLeaveController(ILeaveService leaveService)
        => _leaveService = leaveService;

    private int AdminId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] LeaveFilterDto filter)
    {
        var result = await _leaveService.GetAllLeavesAsync(filter);
        return Ok(result);
    }

    [HttpPatch("{id}/review")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewLeaveDto dto)
    {
        var result = await _leaveService.ReviewLeaveAsync(id, AdminId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}