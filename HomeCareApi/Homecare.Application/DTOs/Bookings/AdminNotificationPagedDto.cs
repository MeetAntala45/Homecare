namespace Homecare.Application.DTOs.Bookings;

public class AdminNotificationPagedDto
{
    public List<AdminNotificationDto> Items { get; set; } = [];
    public int UnreadCount { get; set; }
}

