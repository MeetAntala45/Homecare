namespace Homecare.Application.DTOs.Bookings;

public class PartnerNotificationPagedDto
{
    public List<PartnerNotificationDto> Items { get; set; } = [];
    public int UnreadCount { get; set; }
}
