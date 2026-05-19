using Homecare.Application.Interfaces.Auth;
using Homecare.Application.Services;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.API.BackgroundServices;

public class BookingReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingReminderBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    private static readonly TimeSpan MorningWindow = new(10, 0, 0);
    private static readonly TimeSpan EveningWindow = new(18, 0, 0);
    private static readonly TimeOnly MorningSlotStart = new(9, 0, 0);
    private static readonly TimeOnly MorningSlotEnd = new(12, 0, 0);

    private static readonly string[] AllowedEmailDomains = ["@etatvasoft.com", "@tatvasoft.com"];

    public BookingReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingReminderBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue<bool>("Features:BookingReminderEnabled");

        if (!enabled)
        {
            _logger.LogInformation("BookingReminderBackgroundService is disabled.");
            return;
        }

        _logger.LogInformation("BookingReminderBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            var morningToday = DateTime.Today.Add(MorningWindow);
            var eveningToday = DateTime.Today.Add(EveningWindow);

            DateTime nextRun;
            bool isEveningRun;

            if (now < morningToday)
            {
                nextRun = morningToday;
                isEveningRun = false;
            }
            else if (now < eveningToday)
            {
                nextRun = eveningToday;
                isEveningRun = true;
            }
            else
            {
                nextRun = morningToday.AddDays(1);
                isEveningRun = false;
            }

            var delay = nextRun - now;

            _logger.LogInformation(
                "Next reminder run: {NextRun} ({Window} window). Sleeping for {Delay}.",
                nextRun,
                isEveningRun ? "Evening" : "Morning",
                delay);

            await Task.Delay(delay, stoppingToken);

            try
            {
                if (isEveningRun)
                    await SendEveningRemindersAsync(stoppingToken);
                else
                    await SendMorningRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in BookingReminderBackgroundService.");
            }
        }

        _logger.LogInformation("BookingReminderBackgroundService stopped.");
    }

    private async Task SendMorningRemindersAsync(CancellationToken stoppingToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        _logger.LogInformation("Morning window: afternoon slots on {Date}.", today);

        using var scope = _scopeFactory.CreateScope();
        var (context, emailService, templateService) = ResolveServices(scope);

        var bookings = await context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Service)
            .Include(b => b.Partner)
            .Include(b => b.Address)
            .Where(b =>
                !b.IsDeleted &&
                b.SlotDate == today &&
                b.BookingStatus == BookingStatus.Pending &&
                b.SlotStartTime >= MorningSlotEnd)
            .ToListAsync(stoppingToken);

        if (bookings.Count > 0)
        {
            _logger.LogDebug("Booking IDs: {Ids}",
                string.Join(", ", bookings.Select(b => b.Id)));
        }

        _logger.LogInformation("Morning window: {Count} booking(s) found.", bookings.Count);

        await SendBatchAsync(bookings, "today", emailService, templateService);
    }

    private async Task SendEveningRemindersAsync(CancellationToken stoppingToken)
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        _logger.LogInformation("Evening window: morning slots on {Date}.", tomorrow);

        using var scope = _scopeFactory.CreateScope();
        var (context, emailService, templateService) = ResolveServices(scope);

        var bookings = await context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Service)
            .Include(b => b.Partner)
            .Include(b => b.Address)
            .Where(b =>
                !b.IsDeleted &&
                b.SlotDate == tomorrow &&
                b.BookingStatus == BookingStatus.Pending &&
                b.SlotStartTime >= MorningSlotStart &&
                b.SlotStartTime < MorningSlotEnd)
            .ToListAsync(stoppingToken);

        if (bookings.Count > 0)
        {
            _logger.LogDebug("Booking IDs: {Ids}",
                string.Join(", ", bookings.Select(b => b.Id)));
        }

        _logger.LogInformation("Evening window: {Count} booking(s) found.", bookings.Count);

        await SendBatchAsync(bookings, "tomorrow", emailService, templateService);
    }

    private async Task SendBatchAsync(
        List<Booking> bookings,
        string scheduleContext,
        IEmailService emailService,
        EmailTemplateService templateService)
    {
        foreach (var booking in bookings)
        {
            try
            {
                var partnerName = booking.Partner?.FullName ?? "To be assigned";
                var partnerMobile = booking.Partner?.MobileNumber ?? "—";
                var customerMobile = booking.Customer!.MobileNumber ?? "—";
                var slotDate = booking.SlotDate.ToString("dd MMM yyyy");
                var slotStart = booking.SlotStartTime.ToString("hh:mm tt");
                var slotEnd = booking.SlotEndTime.ToString("hh:mm tt");
                var serviceName = booking.Service.Name;

                if (AllowedEmailDomains.Any(d => booking.Customer!.Email.EndsWith(d)))
                {
                    var customerHtml = templateService.GetTemplate(
                        "BookingReminder.html",
                        new Dictionary<string, string>
                        {
                        { "ScheduleContext", scheduleContext },
                        { "CustomerName",    booking.Customer.Name },
                        { "ServiceName",     serviceName },
                        { "SlotDate",        slotDate },
                        { "SlotStartTime",   slotStart },
                        { "SlotEndTime",     slotEnd },
                        { "PartnerName",     partnerName },
                        { "PartnerMobile",   partnerMobile }
                        });

                    await emailService.SendAsync(
                        booking.Customer.Email,
                        $"Reminder: Your HomeCare booking is scheduled for {scheduleContext} — {serviceName}",
                        customerHtml);

                    _logger.LogInformation(
                        "Customer reminder sent -> {Email} | Booking #{Id}", booking.Customer.Email, booking.Id);
                }
                else
                {
                    _logger.LogInformation("Customer email skipped (domain not allowed) | Booking #{Id}",
                        booking.Id);
                }

                if (booking.Partner != null &&
                    AllowedEmailDomains.Any(d => booking.Partner.Email.EndsWith(d)))
                {
                    var address = booking.Address;

                    var partnerHtml = templateService.GetTemplate(
                        "ServicePartnerBookingReminder.html",
                        new Dictionary<string, string>
                        {
                            { "ScheduleContext", scheduleContext },
                            { "PartnerName",     booking.Partner.FullName },
                            { "ServiceName",     serviceName },
                            { "SlotDate",        slotDate },
                            { "SlotStartTime",   slotStart },
                            { "SlotEndTime",     slotEnd },
                            { "CustomerName",    booking.Customer.Name },
                            { "CustomerMobile",  customerMobile },
                            { "AddressLabel",    address.Label ?? "Home" },
                            { "HouseFlatNo",     address.HouseFlatNo },
                            { "DisplayName",     address.DisplayName ?? string.Empty },
                            { "Landmark",        address.Landmark }
                        });

                    await emailService.SendAsync(
                        booking.Partner.Email,
                        $"Assignment Reminder: Service scheduled for {scheduleContext} — {serviceName}",
                        partnerHtml);

                    _logger.LogInformation(
                        "Partner reminder sent -> {Email} | Booking #{Id}", booking.Partner.Email, booking.Id);
                }
                else
                {
                    _logger.LogInformation("Partner email skipped (domain not allowed) | Booking #{Id}",
                        booking.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for booking #{Id}.", booking.Id);
            }
        }
    }

    private static (AppDbContext, IEmailService, EmailTemplateService) ResolveServices(IServiceScope scope) =>
    (
        scope.ServiceProvider.GetRequiredService<AppDbContext>(),
        scope.ServiceProvider.GetRequiredService<IEmailService>(),
        scope.ServiceProvider.GetRequiredService<EmailTemplateService>()
    );
}