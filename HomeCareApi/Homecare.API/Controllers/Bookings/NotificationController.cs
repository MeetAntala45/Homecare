using System.Security.Claims;
using Homecare.Application.Interfaces.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Bookings
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetAdminId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var result = await _notificationService.GetNotificationsAsync(GetAdminId());
            return Ok(result);
        }

        [HttpPatch("mark-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var result = await _notificationService.MarkAllReadAsync(GetAdminId());
            return Ok(result);
        }

        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkOneRead(int id)
        {
            var result = await _notificationService.MarkOneReadAsync(GetAdminId(), id);
            return Ok(result);
        }

        [HttpGet("view-all")]
        public async Task<IActionResult> ViewAll()
        {
            var result = await _notificationService.ViewAllAsync(GetAdminId());
            return Ok(result);
        }
    }
}
