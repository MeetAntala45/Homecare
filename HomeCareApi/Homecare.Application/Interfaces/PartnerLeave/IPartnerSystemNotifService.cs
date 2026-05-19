
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Domain.Enums;

namespace Homecare.Application.Interfaces.Notification;

public interface IPartnerSystemNotifService
{
    Task SendAsync(
        int partnerId,
        string title,
        string message,
        PartnerSystemNotifType type,
        int? referenceId = null,
        string? referenceType = null);

    Task<List<PartnerSystemNotifDto>> GetAllAsync(int partnerId);
    Task<int> GetUnreadCountAsync(int partnerId);
    Task MarkReadAsync(int notifId, int partnerId);
    Task MarkAllReadAsync(int partnerId);
}