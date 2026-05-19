
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Application.Hubs;
using Homecare.Application.Interfaces.Notification;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Notification;

public class AdminSystemNotifService : IAdminSystemNotifService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<BookingHub> _hub;

    public AdminSystemNotifService(
        AppDbContext context,
        IHubContext<BookingHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task SendToAllAdminsAsync(
        string title,
        string message,
        AdminSystemNotifType type,
        int? referenceId = null,
        string? referenceType = null,
        int? fromPartnerId = null,
        string? fromPartnerName = null)
    {
        var notif = new AdminSystemNotification
        {
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            FromPartnerId = fromPartnerId,
            FromPartnerName = fromPartnerName,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminSystemNotifications.Add(notif);
        await _context.SaveChangesAsync();

        var dto = new AdminSystemNotifDto
        {
            Id = notif.Id,
            Title = title,
            Message = message,
            TypeId = (int)type,
            Type = type.ToString(),
            IsRead = false,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            FromPartnerId = fromPartnerId,
            FromPartnerName = fromPartnerName,
            CreatedAt = notif.CreatedAt.ToString("dd MMM yyyy, hh:mm tt"),
            TimeAgo = "Just now"
        };

        await _hub.Clients
            .Group("Admins")
            .SendAsync("ReceiveAdminSystemNotification", dto);
    }

    public async Task<List<AdminSystemNotifDto>> GetAllAsync()
    {
        return await _context.AdminSystemNotifications
            .OrderByDescending(n => n.Id)
            .Take(50)
            .Select(n => new AdminSystemNotifDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                TypeId = (int)n.Type,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                ReferenceId = n.ReferenceId,
                ReferenceType = n.ReferenceType,
                FromPartnerId = n.FromPartnerId,
                FromPartnerName = n.FromPartnerName,
                CreatedAt = n.CreatedAt.ToString("dd MMM yyyy, hh:mm tt"),
                TimeAgo = GetTimeAgo(n.CreatedAt)
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _context.AdminSystemNotifications
            .CountAsync(n => !n.IsRead);
    }

    public async Task MarkReadAsync(int notifId)
    {
        var notif = await _context.AdminSystemNotifications.FindAsync(notifId);
        if (notif is null) return;
        notif.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync()
    {
        var items = await _context.AdminSystemNotifications
            .Where(n => !n.IsRead)
            .ToListAsync();

        items.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();
    }

    private static string GetTimeAgo(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        return $"{(int)diff.TotalDays}d ago";
    }
}