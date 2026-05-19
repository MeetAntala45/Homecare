using Homecare.Application.Constants;
using Homecare.Application.DTOs.Bookings;
using Homecare.Application.DTOs.Notifications;
using Homecare.Application.Interfaces.Bookings;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Bookings;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveNotificationAsync(BookingNotificationDto dto)
    {
        var notification = new AdminNotification
        {
            BookingId = dto.BookingId,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            ServiceName = dto.ServiceName,
            PaymentMethod = dto.PaymentMethod,
            PaymentMethodValue = dto.PaymentMethodValue,
            SlotDate = dto.SlotDate,
            SlotTime = dto.SlotTime,
            Amount = dto.Amount,
            Message = dto.Message,
            CreatedAt = dto.CreatedAt
        };

        _context.AdminNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<ApiResponse<AdminNotificationPagedDto>> GetNotificationsAsync(int adminId)
    {
        var items = await _context.AdminNotifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new AdminNotificationDto
            {
                Id = n.Id,
                BookingId = n.BookingId,
                CustomerId = n.CustomerId,
                CustomerName = n.CustomerName,
                ServiceName = n.ServiceName,
                PaymentMethod = n.PaymentMethod,
                PaymentMethodValue = n.PaymentMethodValue,
                SlotDate = n.SlotDate,
                SlotTime = n.SlotTime,
                Amount = n.Amount,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.ReadBy.Any(r => r.AdminId == adminId)
            })
            .Where(n => !n.IsRead)
            .ToListAsync();

        var unreadCount = await _context.AdminNotifications
            .CountAsync(n => !n.ReadBy.Any(r => r.AdminId == adminId));

        return ApiResponse<AdminNotificationPagedDto>.SuccessResponse(
            "Notifications fetched.",
            new AdminNotificationPagedDto
            {
                Items = items,
                UnreadCount = unreadCount
            });
    }

    public async Task<ApiResponse<bool>> MarkAllReadAsync(int adminId)
    {
        var unreadIds = await _context.AdminNotifications
            .Where(n => !n.ReadBy.Any(r => r.AdminId == adminId))
            .Select(n => n.Id)
            .ToListAsync();

        if (unreadIds.Count == 0)
            return ApiResponse<bool>.SuccessResponse("Nothing to mark.", true);

        var reads = unreadIds.Select(id => new AdminNotificationRead
        {
            NotificationId = id,
            AdminId = adminId,
            ReadAt = DateTime.UtcNow
        });

        _context.AdminNotificationReads.AddRange(reads);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Marked as read.", true);
    }

    public async Task<ApiResponse<AdminNotificationPagedDto>> ViewAllAsync(int adminId)
    {
        var unreadCount = await _context.AdminNotifications
            .CountAsync(n => !n.ReadBy.Any(r => r.AdminId == adminId));

        var items = await _context.AdminNotifications
        .OrderByDescending(n => n.CreatedAt)
        .Select(n => new AdminNotificationDto {
            Id = n.Id,
                BookingId = n.BookingId,
                CustomerId = n.CustomerId,
                CustomerName = n.CustomerName,
                ServiceName = n.ServiceName,
                PaymentMethod = n.PaymentMethod,
                PaymentMethodValue = n.PaymentMethodValue,
                SlotDate = n.SlotDate,
                SlotTime = n.SlotTime,
                Amount = n.Amount,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.ReadBy.Any(r => r.AdminId == adminId)
        }).Where(n => !n.IsRead)
        .ToListAsync();

        return ApiResponse<AdminNotificationPagedDto>.SuccessResponse("All notifications fetched", new AdminNotificationPagedDto{
            UnreadCount = unreadCount,
            Items = items 
        });
            
    }

    public async Task<ApiResponse<bool>> MarkOneReadAsync(int adminId, int notificationId)
    {
        var alreadyRead = await _context.AdminNotificationReads
            .AnyAsync(r => r.AdminId == adminId && r.NotificationId == notificationId);

        if (alreadyRead)
            return ApiResponse<bool>.SuccessResponse("Already read.", true);

        _context.AdminNotificationReads.Add(new AdminNotificationRead
        {
            NotificationId = notificationId,
            AdminId = adminId,
            ReadAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse("Marked as read.", true);
    }
}
