using System.Security.Claims;
using Homecare.Application.Interfaces.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Bookings
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PartnerNotificationController : ControllerBase
    {
        private readonly IPartnerNotificationService _notificationService;

    public PartnerNotificationController(IPartnerNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int GetPartnerId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _notificationService.GetNotificationsAsync(GetPartnerId());
        return Ok(result);
    }

    [HttpPatch("mark-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var result = await _notificationService.MarkAllReadAsync(GetPartnerId());
        return Ok(result);
    }

    [HttpPatch("{id}/mark-read")]
    public async Task<IActionResult> MarkOneRead(int id)
    {
        var result = await _notificationService.MarkOneReadAsync(GetPartnerId(), id);
        return Ok(result);
    }
    }
}
