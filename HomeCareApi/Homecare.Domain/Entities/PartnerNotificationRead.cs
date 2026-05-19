namespace Homecare.Domain.Entities;

public class PartnerNotificationRead
{
    public int NotificationId { get; set; }
    public int PartnerId { get; set; }
    public DateTime ReadAt { get; set; }

    public PartnerNotification Notification { get; set; } = null!;
    public ServicePartner Partner { get; set; } = null!;
}
