
using Homecare.Application.Constants;
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Application.Interfaces.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers;

[ApiController]
[Route("api/admin/system-notifications")]
[Authorize(Roles = "Admin, SuperAdmin")]
public class AdminSystemNotifController : ControllerBase
{
    private readonly IAdminSystemNotifService _service;

    public AdminSystemNotifController(IAdminSystemNotifService service)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(ApiResponse<List<AdminSystemNotifDto>>
            .SuccessResponse("Fetched.", data));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var count = await _service.GetUnreadCountAsync();
        return Ok(ApiResponse<int>.SuccessResponse("Fetched.", count));
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _service.MarkReadAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("Marked as read.", "OK"));
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _service.MarkAllReadAsync();
        return Ok(ApiResponse<string>.SuccessResponse("All marked as read.", "OK"));
    }
}