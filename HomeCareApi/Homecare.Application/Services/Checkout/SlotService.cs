using Homecare.Application.DTOs.Checkout;
using Homecare.Application.Interfaces.Checkout;
using Homecare.Application.Constants.Checkout;
using Homecare.Data;
using Homecare.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Homecare.Application.Services.Checkout;

public class SlotService : ISlotService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SlotService> _logger;

    public SlotService(AppDbContext context, ILogger<SlotService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static readonly Dictionary<string, (TimeOnly Start, TimeOnly End)> SessionMap = new()
    {
        { "Morning",   (new TimeOnly(9,  0), new TimeOnly(12, 0)) },
        { "Afternoon", (new TimeOnly(12, 0), new TimeOnly(15, 0)) },
        { "Evening",   (new TimeOnly(15, 0), new TimeOnly(18, 0)) },
        { "Night",     (new TimeOnly(18, 0), new TimeOnly(21, 0)) }
    };
    public async Task<List<SlotResponseDto>> GetSlots(GetSlotsRequestDto dto, int customerId)
    {
        if (dto.Date == default)
            throw new Exception("Date is required.");

        if (!SessionMap.ContainsKey(dto.Session))
            throw new Exception("Invalid session.");

        var service = await _context.Services
            .FirstOrDefaultAsync(x => x.Id == dto.ServiceId)
            ?? throw new Exception(CheckoutSlotMessages.ServiceNotFound);

        int duration = service.DurationMin;
        int subCategoryId = service.SubCategoryId;

        var offeredPartnerIds = await _context.PartnerServicesOffered
            .AsNoTracking()
            .Where(x => x.SubCategoryId == subCategoryId && x.IsActive && !x.IsDeleted)
            .Select(x => x.PartnerId)
            .Distinct()
            .ToListAsync();

        if (!offeredPartnerIds.Any())
            return new List<SlotResponseDto>();

        var customerEmail = await _context.Customers
            .Where(c => c.Id == customerId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new Exception("Customer email not found");
        }

        var normalizedCustomerEmail = customerEmail.Trim().ToLower();
        var activePartnerIds = await _context.ServicePartners
            .AsNoTracking()
            .Where(p =>
                offeredPartnerIds.Contains(p.Id) &&
                !p.IsDeleted &&
                p.Email.ToLower() != normalizedCustomerEmail &&
                (p.Status == PartnerStatus.Active || p.Status == PartnerStatus.Onleave) &&
                !_context.PartnerLeaves.Any(l =>
                    l.PartnerId == p.Id &&
                    l.Status == LeaveStatus.Approved &&
                    dto.Date >= l.FromDate &&
                    dto.Date <= l.ToDate
                )
            )
            .Select(p => p.Id)
            .ToListAsync();

        if (!activePartnerIds.Any())
            return new List<SlotResponseDto>();

        int totalPartners = activePartnerIds.Count;

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.SlotDate == dto.Date &&
                b.PartnerId != null &&
                activePartnerIds.Contains(b.PartnerId!.Value) &&
                b.BookingStatus != BookingStatus.Cancelled &&
                b.BookingStatus != BookingStatus.Completed)
            .Select(b => new { b.PartnerId, b.SlotStartTime, b.SlotEndTime })
            .ToListAsync();

        var customerBookings = new List<dynamic>();

        customerBookings = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.ServiceId == dto.ServiceId &&
                b.CustomerId == customerId &&
                b.SlotDate == dto.Date &&
                b.AddressId == dto.AddressId &&
                b.BookingStatus != BookingStatus.Cancelled &&
                b.BookingStatus != BookingStatus.Completed)
            .Select(b => new { b.SlotStartTime, b.SlotEndTime })
            .ToListAsync<dynamic>();

        var now = TimeOnly.FromDateTime(DateTime.Now);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dayStart = new TimeOnly(9, 0);
        var dayEnd = new TimeOnly(21, 0);
        var requestedSession = SessionMap[dto.Session];
        var result = new List<SlotResponseDto>();
        var current = dayStart;


        while (current < dayEnd)
        {
            var slotEnd = current.AddMinutes(duration);

            if (current >= requestedSession.Start && current < requestedSession.End)
            {
                bool isPast = dto.Date == today && slotEnd <= now;

                int busyPartners = bookings
                    .Where(b => b.SlotStartTime < slotEnd && b.SlotEndTime > current.AddMinutes(-15))
                    .Select(b => b.PartnerId)
                    .Distinct()
                    .Count();

                bool alreadyBookedByCustomer = customerBookings.Any(b =>
                    current < (TimeOnly)b.SlotEndTime && slotEnd > (TimeOnly)b.SlotStartTime);


                result.Add(new SlotResponseDto
                {
                    StartTime = current,
                    EndTime = slotEnd,
                    Available = !isPast && !alreadyBookedByCustomer && busyPartners < totalPartners
                });
            }

            current = current.AddMinutes(duration + 15);
        }

        return result;
    }
    public async Task<List<DateAvailabilityDto>> GetAvailableDates(int serviceId, int customerId)
    {
        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == serviceId);

        if (service == null)
            return new List<DateAvailabilityDto>();

        int subCategoryId = service.SubCategoryId;
        int duration = service.DurationMin;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var dateTo = today.AddDays(4);


        var offeredPartnerIds = await _context.PartnerServicesOffered
            .AsNoTracking()
            .Where(x => x.SubCategoryId == subCategoryId && x.IsActive && !x.IsDeleted)
            .Select(x => x.PartnerId)
            .Distinct()
            .ToListAsync();

        if (!offeredPartnerIds.Any())
        {
            return Enumerable.Range(0, 5)
                .Select(i => new DateAvailabilityDto
                {
                    Date = today.AddDays(i),
                    HasSlot = false,
                    AllBooked = false
                })
                .ToList();
        }


        var customerEmail = await _context.Customers
            .Where(c => c.Id == customerId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new Exception("Customer email not found");

        var normalizedCustomerEmail = customerEmail.Trim().ToLower();

        var baseEligiblePartners = await _context.ServicePartners
            .AsNoTracking()
            .Where(p =>
                offeredPartnerIds.Contains(p.Id) &&
                !p.IsDeleted &&
                p.Email.ToLower() != normalizedCustomerEmail &&
                (p.Status == PartnerStatus.Active || p.Status == PartnerStatus.Onleave))
            .Select(p => p.Id)
            .ToListAsync();


        var allLeaves = await _context.PartnerLeaves
            .AsNoTracking()
            .Where(l =>
                baseEligiblePartners.Contains(l.PartnerId) &&
                l.Status == LeaveStatus.Approved &&
                l.FromDate <= dateTo &&
                l.ToDate >= today)
            .Select(l => new { l.PartnerId, l.FromDate, l.ToDate })
            .ToListAsync();

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.SlotDate >= today &&
                b.SlotDate <= dateTo &&
                b.PartnerId != null &&
                baseEligiblePartners.Contains(b.PartnerId!.Value) &&
                b.BookingStatus != BookingStatus.Cancelled &&
                b.BookingStatus != BookingStatus.Completed)
            .Select(b => new { b.SlotDate, b.PartnerId, b.SlotStartTime, b.SlotEndTime })
            .ToListAsync();

        var result = new List<DateAvailabilityDto>();

        for (int i = 0; i < 5; i++)
        {
            var date = today.AddDays(i);

            var partnersOnLeave = allLeaves
                .Where(l => date >= l.FromDate && date <= l.ToDate)
                .Select(l => l.PartnerId)
                .ToHashSet();

            var activePartnerIds = baseEligiblePartners
                .Where(id => !partnersOnLeave.Contains(id))
                .ToList();

            int totalPartners = activePartnerIds.Count;

            _logger.LogInformation(
                "Date {Date}: totalPartners={Total}, onLeave={OnLeave}",
                date, totalPartners, partnersOnLeave.Count);

            if (totalPartners == 0)
            {
                result.Add(new DateAvailabilityDto
                {
                    Date = date,
                    HasSlot = false,
                    AllBooked = false
                });
                continue;
            }

            bool hasSlot = false;
            var dayBookings = bookings
                .Where(b => b.SlotDate == date && activePartnerIds.Contains(b.PartnerId!.Value))
                .ToList();

            var current = new TimeOnly(9, 0);
            var dayEnd = new TimeOnly(21, 0);

            while (current < dayEnd)
            {
                var slotEnd = current.AddMinutes(duration);
                bool isPast = date == today && slotEnd <= now;

                if (!isPast)
                {
                    int busyPartners = dayBookings
                        .Where(b => b.SlotStartTime < slotEnd && b.SlotEndTime > current.AddMinutes(-15))
                        .Select(b => b.PartnerId)
                        .Distinct()
                        .Count();

                    if (busyPartners < totalPartners)
                    {
                        hasSlot = true;
                        break;
                    }
                }

                current = current.AddMinutes(duration + 15);
            }

            result.Add(new DateAvailabilityDto
            {
                Date = date,
                HasSlot = hasSlot,
                AllBooked = !hasSlot
            });
        }

        return result;
    }

    public async Task<Dictionary<int, bool>> GetServicePartnerAvailability()
    {
        var activePartnerSubCategories = await _context.ServicePartners
            .AsNoTracking()
            .Where(p =>
            (p.Status == PartnerStatus.Active || p.Status == PartnerStatus.Onleave) && !p.IsDeleted)
            .Join(_context.PartnerServicesOffered,
                p => p.Id,
                o => o.PartnerId,
                (p, o) => o.SubCategoryId)
            .Distinct()
            .ToListAsync();

        var result = await _context.Services
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                HasPartner = activePartnerSubCategories.Contains(s.SubCategoryId)
            })
            .ToDictionaryAsync(x => x.Id, x => x.HasPartner);

        return result;

    }
}