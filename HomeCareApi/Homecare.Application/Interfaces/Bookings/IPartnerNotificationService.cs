using Homecare.Application.Constants;
using Homecare.Application.DTOs.Bookings;
using Homecare.Application.DTOs.Notifications;

namespace Homecare.Application.Interfaces.Bookings;

public interface IPartnerNotificationService
{
    Task SaveNotificationAsync(BookingNotificationDto dto);
    Task<ApiResponse<PartnerNotificationPagedDto>> GetNotificationsAsync(int partnerId);
    Task<ApiResponse<bool>> MarkAllReadAsync(int partnerId);
    Task<ApiResponse<bool>> MarkOneReadAsync(int partnerId, int notificationId);
}
