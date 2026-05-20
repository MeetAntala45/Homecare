using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.Checkout;
using Homecare.Application.DTOs.Checkout;
using Homecare.Application.Interfaces.Checkout;
using Homecare.Application.Services.Offers;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Homecare.Application.Hubs;
using Homecare.Application.DTOs.Notifications;
using Microsoft.Extensions.Logging;
using Homecare.Application.Interfaces.Bookings;
using Homecare.Application.Interfaces.Referral;

namespace Homecare.Application.Services.Checkout;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly RuleEngine _ruleEngine;
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly IPartnerNotificationService _partnerNotificationService;

    private readonly ILogger<BookingService> _logger;
    // Add to constructor parameters:
    private readonly IReferralService _referralService;

    public BookingService(
        AppDbContext context,
        RuleEngine ruleEngine,
        IHubContext<BookingHub> hubContext,
        ILogger<BookingService> logger,
        INotificationService notificationService,
        IPartnerNotificationService partnerNotificationService,
        IReferralService referralService)   // ← ADD
    {
        _context = context;
        _ruleEngine = ruleEngine;
        _hubContext = hubContext;
        _logger = logger;
        _notificationService = notificationService;
        _partnerNotificationService = partnerNotificationService;
        _referralService = referralService;          // ← ADD
    }


    public async Task<ApiResponse<CreateBookingResponseDto>> CreateBooking(
    CreateBookingRequestDto dto, int customerId)
    {

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = TimeOnly.FromDateTime(DateTime.Now);

        if (dto.SlotDate < today ||
           (dto.SlotDate == today && dto.SlotEndTime <= now))
            return ApiResponse<CreateBookingResponseDto>.Fail(CheckoutBookingMessages.SlotInPast);


        var service = await _context.Services
            .FirstOrDefaultAsync(x => x.Id == dto.ServiceId)
            ?? throw new Exception(CheckoutBookingMessages.ServiceNotFound);


        var address = await _context.Addresses
            .FirstOrDefaultAsync(x => x.Id == dto.AddressId && x.CustomerId == customerId)
            ?? throw new Exception(CheckoutBookingMessages.AddressNotFound);

        bool duplicateExistForSameCustomer = await _context.Bookings.AnyAsync(b =>
           b.CustomerId == customerId &&
           b.ServiceId == dto.ServiceId &&
           b.SlotDate == dto.SlotDate &&
           b.AddressId == dto.AddressId &&
           b.SlotStartTime == dto.SlotStartTime &&
           b.BookingStatus != BookingStatus.Cancelled &&
           b.BookingStatus != BookingStatus.Completed);

        if (duplicateExistForSameCustomer)
            return ApiResponse<CreateBookingResponseDto>.Fail(CheckoutBookingMessages.SlotAlreadyBookedByYou);

        bool duplicateExists = await _context.Bookings.AnyAsync(b =>
            b.ServiceId == dto.ServiceId &&
            b.SlotDate == dto.SlotDate &&
            b.AddressId == dto.AddressId &&
            b.SlotStartTime == dto.SlotStartTime &&
            b.BookingStatus != BookingStatus.Cancelled &&
            b.BookingStatus != BookingStatus.Completed);

        if (duplicateExists)
            return ApiResponse<CreateBookingResponseDto>.Fail(CheckoutBookingMessages.SlotAlreadyBooked);

        int? assignedPartnerId = await PickAvailablePartner(
            service.SubCategoryId,
            dto.ServiceId,
            dto.SlotDate,
            dto.SlotStartTime,
            dto.SlotEndTime,
            excludeBookingId: null,
            customerId);

        if (assignedPartnerId == null)
            return ApiResponse<CreateBookingResponseDto>.Fail(CheckoutBookingMessages.SlotUnavailable);

        decimal servicePrice = service.Price;
        decimal taxPct = TaxConstants.TaxPct;
        decimal taxAmount = Math.Round(servicePrice * taxPct / 100, 2);
        decimal discountAmount = 0;
        int? couponId = null;
        string? couponCode = null;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var coupon = await _context.coupons
                .Include(c => c.Conditions)
                    .ThenInclude(cc => cc.ConditionType)
                .FirstOrDefaultAsync(c =>
                    c.CouponCode == dto.CouponCode.ToUpper() &&
                    c.Status == CouponStatus.Active &&
                    !c.IsDeleted);

            if (coupon == null)
            {
                return ApiResponse<CreateBookingResponseDto>.Fail(CheckoutBookingMessages.CouponNotFound);
            }

            if (coupon != null)
            {
                couponCode = coupon.CouponCode;
                var userBookingCount = await _context.Bookings
                    .CountAsync(b => b.CustomerId == customerId);

                var userCouponUses = await _context.Bookings
                .CountAsync(b =>
                    b.CustomerId == customerId &&
                    b.CouponId == coupon.Id &&
                    b.PaymentStatus != PaymentStatus.Failed &&
                    b.BookingStatus != BookingStatus.Cancelled);

                var ctx = new CouponCheckContext
                {
                    CartTotal = servicePrice,
                    UserBookingCount = userBookingCount,
                    UserCouponUses = userCouponUses,
                    SlotDayOfWeek = dto.SlotDate.DayOfWeek.ToString().ToLower(),
                    SlotTime = dto.SlotStartTime.ToTimeSpan(),
                    SlotDate = dto.SlotDate.ToDateTime(TimeOnly.MinValue),
                    ServiceSubCategoryId = service.SubCategoryId
                };

                var allPass = coupon.Conditions.All(c =>
                {
                    if (c.ConditionType == null) return true;
                    return _ruleEngine.Evaluate(c, ctx);
                });

                if (!allPass)
                    return ApiResponse<CreateBookingResponseDto>.Fail(CheckoutBookingMessages.CouponNotApplicable);

                couponId = coupon.Id;
                discountAmount = Math.Round(servicePrice * coupon.DiscountPct / 100, 2);
                discountAmount = Math.Min(discountAmount, servicePrice);

            }
        }

        decimal totalAmount = servicePrice + taxAmount - discountAmount;

        decimal refereeDiscount = await _referralService.GetRefereeFirstBookingDiscountAsync(customerId);
        if (refereeDiscount > 0 && totalAmount - refereeDiscount > 0)
        {
            totalAmount -= refereeDiscount;
            discountAmount += refereeDiscount;
        }
        else
        {
            refereeDiscount = 0;
        }


        var booking = new Booking
        {
            CustomerId = customerId,
            ServiceId = dto.ServiceId,
            AddressId = dto.AddressId,
            PartnerId = assignedPartnerId,
            CouponId = couponId,
            SlotDate = dto.SlotDate,
            SlotStartTime = dto.SlotStartTime,
            SlotEndTime = dto.SlotEndTime,
            ServicePrice = servicePrice,
            TaxPct = taxPct,
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            PaymentMethod = dto.PaymentMethod,
            BookingStatus = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            CreatedBy = customerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync();

        }
        catch (DbUpdateException)
        {

            throw new Exception(CheckoutBookingMessages.SlotUnavailable);
        }
        decimal walletAmountUsed = 0;
        if (dto.UseWallet)
        {
            walletAmountUsed = await _referralService.UseWalletAsync(
                customerId, booking.TotalAmount, booking.Id);

            if (walletAmountUsed > 0)
            {
                booking.TotalAmount -= walletAmountUsed;
                booking.ModifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        if (dto.PaymentMethod == PaymentMethod.Cash)
        {
            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = totalAmount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                TransactionId = $"TXN-{booking.Id}-{DateTime.UtcNow.Ticks}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await NotifyAdminsAsync(booking, customerId);
        }
        var data = new CreateBookingResponseDto
        {
            BookingId = booking.Id,
            SlotDate = booking.SlotDate,
            SlotStartTime = booking.SlotStartTime,
            SlotEndTime = booking.SlotEndTime,
            TotalAmount = booking.TotalAmount,
            PaymentMethod = dto.PaymentMethod,
            BookingStatus = booking.BookingStatus,
            PaymentStatus = booking.PaymentStatus,
            CouponCode = couponCode,
            WalletAmountUsed = walletAmountUsed   // ← ADD
        };

        string message = dto.PaymentMethod == Homecare.Domain.Enums.PaymentMethod.DebitCard
                ? CheckoutBookingMessages.BookingCreatedCard
                : CheckoutBookingMessages.BookingCreatedCash;


        _logger.LogInformation("CouponCode response: {CouponCode}", data.CouponCode);

        return ApiResponse<CreateBookingResponseDto>.SuccessResponse(message, data);
    }

    public async Task<ApiResponse<Dictionary<int, int>>> GetAllServiceBookingCountsAsync()
    {
        try
        {
            var count = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.BookingStatus == BookingStatus.Completed)
                .GroupBy(b => b.ServiceId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return ApiResponse<Dictionary<int, int>>.SuccessResponse(CheckoutBookingMessages.BookingCountSuccess, count);
        }
        catch (Exception ex)
        {
            return ApiResponse<Dictionary<int, int>>.Fail(CheckoutBookingMessages.BookingCountFail, new List<string> { ex.Message });
        }
    }


    public async Task HandlePaymentCallback(PaymentCallbackDto dto)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId)
            ?? throw new Exception(CheckoutBookingMessages.BookingNotFound);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == dto.BookingId)
            ?? throw new Exception(CheckoutBookingMessages.BookingNotFound);

        if (payment.PaymentStatus == PaymentStatus.Paid)
            throw new Exception(CheckoutBookingMessages.PaymentAlreadyDone);

        bool isSuccess = dto.Status.Equals("success", StringComparison.OrdinalIgnoreCase);

        payment.TransactionId = dto.TransactionId;
        payment.PaymentStatus = isSuccess ? PaymentStatus.Paid : PaymentStatus.Failed;
        payment.ModifiedAt = DateTime.UtcNow;


        booking.PaymentStatus = payment.PaymentStatus;
        booking.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        if (!isSuccess)
            throw new Exception(CheckoutBookingMessages.PaymentFailed);
    }

    private async Task<int?> PickAvailablePartner(
    int subCategoryId,
    int serviceId,
    DateOnly slotDate,
    TimeOnly slotStart,
    TimeOnly slotEnd,
    int? excludeBookingId,
    int customerId)
    {
        var offeredPartnerIds = await _context.PartnerServicesOffered
            .Where(x => x.SubCategoryId == subCategoryId && x.IsActive && !x.IsDeleted)
            .Select(x => x.PartnerId)
            .Distinct()
            .ToListAsync();

        if (!offeredPartnerIds.Any())
            throw new Exception(CheckoutBookingMessages.NoPartnersAvailable);
        var customerEmail = await _context.Customers
            .Where(c => c.Id == customerId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync();

        var normalizedCustomerEmail = customerEmail!.Trim().ToLower();

        var activePartnerIds = await _context.ServicePartners
        .Where(p =>
        offeredPartnerIds.Contains(p.Id) &&
        !p.IsDeleted &&
        p.Email != normalizedCustomerEmail &&
        (p.Status == PartnerStatus.Active || p.Status == PartnerStatus.Onleave) &&

        !_context.PartnerLeaves.Any(l =>
            l.PartnerId == p.Id &&
            l.Status == LeaveStatus.Approved &&
            slotDate >= l.FromDate &&
            slotDate <= l.ToDate
        )
    )
    .Select(p => p.Id)
    .ToListAsync();
        if (!activePartnerIds.Any())
            throw new Exception(CheckoutBookingMessages.NoPartnersAvailable);

        var bufferStart = slotStart.AddMinutes(-15);

        var busyQuery = _context.Bookings
            .Where(b =>
                b.SlotDate == slotDate &&
                b.PartnerId != null &&
                activePartnerIds.Contains(b.PartnerId!.Value) &&
                b.BookingStatus != BookingStatus.Cancelled &&
                b.BookingStatus != BookingStatus.Completed &&
                b.SlotStartTime < slotEnd &&
                b.SlotEndTime > bufferStart);

        if (excludeBookingId.HasValue)
            busyQuery = busyQuery.Where(b => b.Id != excludeBookingId.Value);

        var busyPartnerIds = await busyQuery
            .Select(b => b.PartnerId!.Value)
            .Distinct()
            .ToListAsync();

        var availablePartnerIds = activePartnerIds
            .Where(id => !busyPartnerIds.Contains(id))
            .ToList();

        if (!availablePartnerIds.Any())
            return null;


        var todayWorkLoad = await _context.Bookings
            .Where(b =>
                b.SlotDate == slotDate &&
                b.PartnerId != null &&
                availablePartnerIds.Contains(b.PartnerId.Value) &&
                b.BookingStatus != BookingStatus.Cancelled &&
                b.BookingStatus != BookingStatus.Completed)
            .GroupBy(b => b.PartnerId!.Value)
            .Select(g => new
            {
                PartnerId = g.Key,
                TotalMinutes = g.Sum(b => b.SlotEndTime.Hour * 60 + b.SlotEndTime.Minute
            - b.SlotStartTime.Hour * 60 - b.SlotStartTime.Minute)
            })
            .ToListAsync();

        var workMinutesMap = availablePartnerIds.ToDictionary(
        id => id,
        id => todayWorkLoad.FirstOrDefault(x => x.PartnerId == id)?.TotalMinutes ?? 0);

        var lastSlotData = await _context.Bookings
        .Where(b =>
            b.SlotDate == slotDate &&
            b.PartnerId != null &&
            availablePartnerIds.Contains(b.PartnerId!.Value) &&
            b.BookingStatus != BookingStatus.Cancelled &&
            b.BookingStatus != BookingStatus.Completed &&
            b.SlotEndTime <= slotStart)
        .GroupBy(b => b.PartnerId!.Value)
        .Select(g => new
        {
            PartnerId = g.Key,
            LastSlotEndBeforeNewSlot = g.Max(b => b.SlotEndTime)
        })
        .ToListAsync();

        var lastSlotEndMap = availablePartnerIds.ToDictionary(
            id => id,
            id => lastSlotData.FirstOrDefault(x => x.PartnerId == id)
                      ?.LastSlotEndBeforeNewSlot ?? TimeOnly.MinValue);

        return availablePartnerIds
            .OrderBy(id => workMinutesMap[id])
            .ThenBy(id => lastSlotEndMap[id])
            .FirstOrDefault();
    }

    public async Task<ApiResponse<int>> GetTotalBookingByServiceTypeAsync(int serviceTypeId)
    {
        try
        {
            var count = await (
                from b in _context.Bookings
                join s in _context.Services on b.ServiceId equals s.Id
                join sc in _context.SubCategories on s.SubCategoryId equals sc.Id
                join c in _context.Categories on sc.CategoryId equals c.Id
                where b.BookingStatus == BookingStatus.Completed
                    && b.PaymentStatus == PaymentStatus.Paid
                    && c.ServiceTypeId == serviceTypeId
                select b
            )
            .AsNoTracking()
            .CountAsync();

            return ApiResponse<int>.SuccessResponse(
                CheckoutBookingMessages.BookingCountSuccess,
                count
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.Fail(
                CheckoutBookingMessages.BookingCountFail,
                new List<string> { ex.Message }
            );
        }
    }

    private async Task NotifyAdminsAsync(Booking booking, int customerId)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == booking.ServiceId);

        var notification = new BookingNotificationDto
        {
            BookingId = booking.Id,
            PartnerId = booking.PartnerId ?? 0,
            CustomerId = booking.CustomerId,
            CustomerName = customer?.Name ?? "Customer",
            ServiceName = service?.Name ?? "Service",
            PaymentMethod = booking.PaymentMethod.ToString(),
            PaymentMethodValue = (int)booking.PaymentMethod,
            SlotDate = booking.SlotDate.ToString("dd MMM yyyy"),
            SlotTime = booking.SlotStartTime.ToString(@"hh\:mm tt"),
            Amount = booking.TotalAmount,
            Message = $"New booking by {customer?.Name ?? "a customer"} for {service?.Name ?? "a service"}",
            CreatedAt = DateTime.UtcNow,
            Status = booking.BookingStatus.ToString()
        };

        await _notificationService.SaveNotificationAsync(notification);


        if (booking.PartnerId.HasValue)
        {
            await _partnerNotificationService.SaveNotificationAsync(notification);
            await _hubContext.Clients
                .Group($"Partner_{booking.PartnerId.Value}")
                .SendAsync("NewPartnerBooking", notification);
        }

        await _hubContext.Clients
            .Group("Admins")
            .SendAsync("NewBooking", notification);

    }
}
