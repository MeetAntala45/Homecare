using Homecare.Application.Constants;
using Homecare.Application.DTOs.Bookings;
using Homecare.Application.DTOs.Notifications;
using Homecare.Application.Interfaces.Bookings;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Bookings;

public class PartnerNotificationService : IPartnerNotificationService
{
    private readonly AppDbContext _context;

    public PartnerNotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveNotificationAsync(BookingNotificationDto dto)
    {
        var notification = new PartnerNotification
        {
            BookingId = dto.BookingId,
            PartnerId = dto.PartnerId,
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

        _context.PartnerNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<ApiResponse<PartnerNotificationPagedDto>> GetNotificationsAsync(int partnerId)
    {
        var items = await _context.PartnerNotifications
            .Where(n => n.PartnerId == partnerId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new PartnerNotificationDto
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
                IsRead = n.ReadBy.Any(r => r.PartnerId == partnerId),
                Status = _context.Bookings
                .Where(b => b.Id == n.BookingId)
                .Select(b => b.BookingStatus.ToString())
                .FirstOrDefault() ?? "Pending"
            })
            .ToListAsync();

        var unreadCount = await _context.PartnerNotifications
            .CountAsync(n => n.PartnerId == partnerId && !n.ReadBy.Any(r => r.PartnerId == partnerId));

        return ApiResponse<PartnerNotificationPagedDto>.SuccessResponse(
            "Notifications fetched.",
            new PartnerNotificationPagedDto { Items = items, UnreadCount = unreadCount });
    }

    public async Task<ApiResponse<bool>> MarkAllReadAsync(int partnerId)
    {
        var unreadIds = await _context.PartnerNotifications
            .Where(n => n.PartnerId == partnerId && !n.ReadBy.Any(r => r.PartnerId == partnerId))
            .Select(n => n.Id)
            .ToListAsync();

        if (unreadIds.Count == 0)
            return ApiResponse<bool>.SuccessResponse("Nothing to mark.", true);

        var reads = unreadIds.Select(id => new PartnerNotificationRead
        {
            NotificationId = id,
            PartnerId = partnerId,
            ReadAt = DateTime.UtcNow
        });

        _context.PartnerNotificationReads.AddRange(reads);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Marked as read.", true);
    }

    public async Task<ApiResponse<bool>> MarkOneReadAsync(int partnerId, int notificationId)
    {
        var alreadyRead = await _context.PartnerNotificationReads
            .AnyAsync(r => r.PartnerId == partnerId && r.NotificationId == notificationId);

        if (alreadyRead)
            return ApiResponse<bool>.SuccessResponse("Already read.", true);

        _context.PartnerNotificationReads.Add(new PartnerNotificationRead
        {
            NotificationId = notificationId,
            PartnerId = partnerId,
            ReadAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse("Marked as read.", true);
    }
}
