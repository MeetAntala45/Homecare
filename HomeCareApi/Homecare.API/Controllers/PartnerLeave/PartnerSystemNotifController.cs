// Homecare.API/Controllers/PartnerSystemNotifController.cs
using System.Security.Claims;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Application.Interfaces.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers;

[ApiController]
[Route("api/partner/system-notifications")]
[Authorize(Roles = "ServicePartner,Admin")]
public class PartnerSystemNotifController : ControllerBase
{
    private readonly IPartnerSystemNotifService _service;

    public PartnerSystemNotifController(IPartnerSystemNotifService service)
        => _service = service;

    private int PartnerId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync(PartnerId);
        return Ok(ApiResponse<List<PartnerSystemNotifDto>>
            .SuccessResponse("Fetched.", data));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var count = await _service.GetUnreadCountAsync(PartnerId);
        return Ok(ApiResponse<int>.SuccessResponse("Fetched.", count));
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _service.MarkReadAsync(id, PartnerId);
        return Ok(ApiResponse<string>.SuccessResponse("Marked as read.", "OK"));
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _service.MarkAllReadAsync(PartnerId);
        return Ok(ApiResponse<string>.SuccessResponse("All marked as read.", "OK"));
    }
}