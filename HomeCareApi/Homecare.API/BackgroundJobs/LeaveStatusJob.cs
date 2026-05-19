
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.API.BackgroundJobs;

public class LeaveStatusJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LeaveStatusJob> _logger;

    public LeaveStatusJob(
        IServiceScopeFactory scopeFactory,
        ILogger<LeaveStatusJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LeaveStatusJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("LeaveStatusJob running at {Time}", DateTimeOffset.Now);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var today = DateOnly.FromDateTime(DateTime.Today);
                var goOnLeave = await context.PartnerLeaves
                    .Include(l => l.Partner)
                    .Where(l =>
                        l.Status == LeaveStatus.Approved &&
                        l.FromDate <= today &&    
                        l.ToDate >= today &&     
                        l.Partner.Status != PartnerStatus.Onleave)
                    .ToListAsync(stoppingToken);

                foreach (var leave in goOnLeave)
                    leave.Partner.Status = PartnerStatus.Onleave;

                _logger.LogInformation("Partners set to OnLeave: {Count}", goOnLeave.Count);

                var returnFromLeave = await context.PartnerLeaves
                    .Include(l => l.Partner)
                    .Where(l =>
                        l.Status == LeaveStatus.Approved &&
                        l.ToDate < today &&         
                        l.Partner.Status == PartnerStatus.Onleave)
                    .ToListAsync(stoppingToken);

                foreach (var leave in returnFromLeave)
                    leave.Partner.Status = PartnerStatus.Active;

                _logger.LogInformation("Partners returned to Active: {Count}", returnFromLeave.Count);

                if (goOnLeave.Any() || returnFromLeave.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("LeaveStatusJob saved changes successfully.");
                }
                else
                {
                    _logger.LogInformation("LeaveStatusJob: No status changes needed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LeaveStatusJob");
            }

            _logger.LogInformation("LeaveStatusJob cycle complete. Next run in 1 minute.");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("LeaveStatusJob stopped.");
    }
}