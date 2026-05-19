using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Homecare.Application.Hubs;

public class BookingHub : Hub
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BookingHub(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
    }

    public async Task JoinPartnerGroup(int partnerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Partner_{partnerId}");
    }

    public async Task LeavePartnerGroup(int partnerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Partner_{partnerId}");
    }

    public async Task JoinBookingTrackingGroup(int bookingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Tracking_{bookingId}");
    }

    public async Task LeaveBookingTrackingGroup(int bookingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tracking_{bookingId}");
    }
}