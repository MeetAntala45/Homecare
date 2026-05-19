using Homecare.Application.Interfaces.Payments;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace Homecare.API.BackgroundJobs;

public class BookingExpiryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpiryJob> _logger;

    private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(5);

    public BookingExpiryJob(IServiceScopeFactory scopeFactory,
                            ILogger<BookingExpiryJob> logger)
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

                await paymentService.ExpireTimedOutBookingsAsync();

                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var nextExpiry = await context.Bookings
                    .Where(b =>
                        b.PaymentStatus == PaymentStatus.Pending &&
                        b.BookingStatus == BookingStatus.Pending &&
                        b.PaymentMethod != PaymentMethod.Cash &&
                        b.CreatedAt.AddMinutes(5) > DateTime.UtcNow)
                    .OrderBy(b => b.CreatedAt)
                    .Select(b => (DateTime?)b.CreatedAt.AddMinutes(5))
                    .FirstOrDefaultAsync(stoppingToken);

                TimeSpan delay;

                if (nextExpiry.HasValue)
                {
                    var timeUntilExpiry = nextExpiry.Value - DateTime.UtcNow
                        + TimeSpan.FromSeconds(2);

                    delay = timeUntilExpiry < MaxDelay ? timeUntilExpiry : MaxDelay;

                    if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                    _logger.LogDebug(
                        "BookingExpiryJob sleeping {Seconds}s until next expiry at {ExpiryTime}",
                        delay.TotalSeconds, nextExpiry.Value);
                }else
                {
                    delay = MaxDelay;
                    _logger.LogDebug(
                        "BookingExpiryJob: no pending bookings. Sleeping {Minutes} min.",
                        MaxDelay.TotalMinutes);
                }

                await Task.Delay(delay, stoppingToken);
            }
             catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingExpiryJob");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            
        }
    }
}