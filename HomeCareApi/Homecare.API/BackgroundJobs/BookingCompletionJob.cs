using Homecare.Application.Interfaces.Payments;
using Homecare.Application.Interfaces.Referral;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.API.BackgroundJobs;

public class BookingCompletionJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingCompletionJob> _logger;
    private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(5);

    public BookingCompletionJob(IServiceScopeFactory scopeFactory,
                                ILogger<BookingCompletionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var paymentService = scope.ServiceProvider
                    .GetRequiredService<IPaymentService>();

                await paymentService.AutoStartBookingsAsync();
                await paymentService.AutoCompleteBookingsAsync();

                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();
                var referralService = scope.ServiceProvider
                    .GetRequiredService<IReferralService>();


                await ProcessReferrerRewardsAsync(context, referralService);

                var now = DateTime.Now;
                var today = DateOnly.FromDateTime(now);
                var nowTime = TimeOnly.FromDateTime(now);

                var nextStartTime = await context.Bookings
                   .Where(b =>
                       b.BookingStatus == BookingStatus.Pending &&
                       b.SlotDate == today &&
                       b.SlotStartTime > nowTime &&
                       (
                           (b.PaymentMethod == PaymentMethod.DebitCard &&
                            b.PaymentStatus == PaymentStatus.Paid) ||
                           (b.PaymentMethod == PaymentMethod.Cash &&
                            b.PaymentStatus == PaymentStatus.Pending)
                       ))
                   .OrderBy(b => b.SlotStartTime)
                   .Select(b => (TimeOnly?)b.SlotStartTime)
                   .FirstOrDefaultAsync(stoppingToken);

                var nextEndTime = await context.Bookings
                    .Where(b =>
                        (b.BookingStatus == BookingStatus.Pending ||
                         b.BookingStatus == BookingStatus.InProgress) &&
                        b.SlotDate == today &&
                        b.SlotEndTime > nowTime &&
                        (
                            (b.PaymentMethod == PaymentMethod.DebitCard &&
                             b.PaymentStatus == PaymentStatus.Paid) ||
                            (b.PaymentMethod == PaymentMethod.Cash &&
                             b.PaymentStatus == PaymentStatus.Pending)
                        ))
                    .OrderBy(b => b.SlotEndTime)
                    .Select(b => (TimeOnly?)b.SlotEndTime)
                    .FirstOrDefaultAsync(stoppingToken);

                DateTime? nextStartDt = nextStartTime.HasValue
                   ? today.ToDateTime(nextStartTime.Value)
                   : null;

                DateTime? nextEndDt = nextEndTime.HasValue
                    ? today.ToDateTime(nextEndTime.Value)
                    : null;

                DateTime? nextEventAt = (nextStartDt, nextEndDt) switch
                {
                    (null, null) => null,
                    (var s, null) => s,
                    (null, var e) => e,
                    (var s, var e) => s < e ? s : e
                };

                TimeSpan delay;

                if (nextEventAt.HasValue)
                {
                    var timeUntilEvent = nextEventAt.Value - now
                        + TimeSpan.FromSeconds(5);

                    delay = timeUntilEvent < MaxDelay ? timeUntilEvent : MaxDelay;

                    if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                    _logger.LogDebug(
                        "BookingCompletionJob sleeping {Seconds}s until next event at {EventTime}",
                        delay.TotalSeconds, nextEventAt.Value);
                }
                else
                {
                    delay = MaxDelay;
                    _logger.LogDebug(
                        "BookingCompletionJob: no upcoming events today. " +
                        "Sleeping {Minutes} min.", MaxDelay.TotalMinutes);
                }

                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingCompletionJob");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

        }
    }
    private async Task ProcessReferrerRewardsAsync(
    AppDbContext context, IReferralService referralService)
    {
        // Find bookings that just became Completed in the last 10 minutes
        // and have a pending referral on the referee
        var recentlyCompleted = await context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.BookingStatus == BookingStatus.Completed &&
                b.ModifiedAt >= DateTime.UtcNow.AddMinutes(-10))
            .Select(b => b.Id)
            .ToListAsync();

        foreach (var bookingId in recentlyCompleted)
        {
            try
            {
                await referralService.ProcessReferrerRewardAsync(bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process referrer reward for booking {BookingId}", bookingId);
            }
        }
    }
}