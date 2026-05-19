using System;

namespace Homecare.Domain.Entities;

public class AdminNotificationRead
{
    public int NotificationId { get; set; }
    public int AdminId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public AdminNotification Notification { get; set; } = null!;
    public Admin Admin { get; set; } = null!;
}
