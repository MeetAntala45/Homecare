
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Domain.Enums;

namespace Homecare.Application.Interfaces.Notification;

public interface IAdminSystemNotifService
{
    Task SendToAllAdminsAsync(
        string title,
        string message,
        AdminSystemNotifType type,
        int? referenceId = null,
        string? referenceType = null,
        int? fromPartnerId = null,
        string? fromPartnerName = null);

    Task<List<AdminSystemNotifDto>> GetAllAsync();
    Task<int> GetUnreadCountAsync();
    Task MarkReadAsync(int notifId);
    Task MarkAllReadAsync();
}