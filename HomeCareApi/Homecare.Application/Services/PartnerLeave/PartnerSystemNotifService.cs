
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Application.Hubs;
using Homecare.Application.Interfaces.Notification;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Notification;

public class PartnerSystemNotifService : IPartnerSystemNotifService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<BookingHub> _hub;

    public PartnerSystemNotifService(
        AppDbContext context,
        IHubContext<BookingHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task SendAsync(
        int partnerId,
        string title,
        string message,
        PartnerSystemNotifType type,
        int? referenceId = null,
        string? referenceType = null)
    {
        var notif = new PartnerSystemNotification
        {
            PartnerId = partnerId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            CreatedAt = DateTime.UtcNow
        };

        _context.PartnerSystemNotifications.Add(notif);
        await _context.SaveChangesAsync();

        var dto = MapToDto(notif);

        await _hub.Clients
            .Group($"Partner_{partnerId}")
            .SendAsync("ReceiveSystemNotification", dto);
    }

    public async Task<List<PartnerSystemNotifDto>> GetAllAsync(int partnerId)
    {
        return await _context.PartnerSystemNotifications
            .Where(n => n.PartnerId == partnerId)
            .OrderByDescending(n => n.Id)
            .Take(50)
            .Select(n => new PartnerSystemNotifDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                TypeId = (int)n.Type,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                ReferenceId = n.ReferenceId,
                ReferenceType = n.ReferenceType,
                CreatedAt = n.CreatedAt.ToString("dd MMM yyyy, hh:mm tt"),
                TimeAgo = GetTimeAgo(n.CreatedAt)
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int partnerId)
    {
        return await _context.PartnerSystemNotifications
            .CountAsync(n => n.PartnerId == partnerId && !n.IsRead);
    }

    public async Task MarkReadAsync(int notifId, int partnerId)
    {
        var notif = await _context.PartnerSystemNotifications
            .FirstOrDefaultAsync(n => n.Id == notifId && n.PartnerId == partnerId);

        if (notif is null) return;

        notif.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int partnerId)
    {
        var items = await _context.PartnerSystemNotifications
            .Where(n => n.PartnerId == partnerId && !n.IsRead)
            .ToListAsync();

        items.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();
    }

    private static PartnerSystemNotifDto MapToDto(PartnerSystemNotification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        TypeId = (int)n.Type,
        Type = n.Type.ToString(),
        IsRead = n.IsRead,
        ReferenceId = n.ReferenceId,
        ReferenceType = n.ReferenceType,
        CreatedAt = n.CreatedAt.ToString("dd MMM yyyy, hh:mm tt"),
        TimeAgo = "Just now"
    };

    private static string GetTimeAgo(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        return $"{(int)diff.TotalDays}d ago";
    }
}