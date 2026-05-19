using Homecare.Application.Constants;
using Homecare.Application.DTOs.Bookings;
using Homecare.Application.DTOs.Notifications;

namespace Homecare.Application.Interfaces.Bookings;

public interface INotificationService
{
    Task SaveNotificationAsync(BookingNotificationDto dto);
    Task<ApiResponse<AdminNotificationPagedDto>> GetNotificationsAsync(int adminId);
    Task<ApiResponse<AdminNotificationPagedDto>> ViewAllAsync(int adminId);
    Task<ApiResponse<bool>> MarkAllReadAsync(int adminId);
    Task<ApiResponse<bool>> MarkOneReadAsync(int adminId, int notificationId);
}
