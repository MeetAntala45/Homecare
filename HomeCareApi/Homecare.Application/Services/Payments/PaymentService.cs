using Homecare.Application.Constants;
using Homecare.Application.Constants.Payments;
using Homecare.Application.DTOs.Notifications;
using Homecare.Application.DTOs.Payments;
using Homecare.Application.Hubs;
using Homecare.Application.Interfaces.Bookings;
using Homecare.Application.Interfaces.Payments;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Homecare.Application.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly ILogger<PaymentService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IPartnerNotificationService _partnerNotificationService;
    public PaymentService(AppDbContext context, IConfiguration config, IHubContext<BookingHub> hubContext, ILogger<PaymentService> logger,
    INotificationService notificationService, IPartnerNotificationService partnerNotificationService)
    {
        _context = context;
        _config = config;
        _hubContext = hubContext;
        _logger = logger;
        _notificationService = notificationService;
        _partnerNotificationService = partnerNotificationService;
    }

    public async Task<ApiResponse<CheckoutSessionResponseDto>> CreateCheckoutSessionAsync(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);

        if (booking is null)
            return ApiResponse<CheckoutSessionResponseDto>.Fail(PaymentMessages.BookingNotFound);

        if (booking.PaymentStatus == PaymentStatus.Paid)
            return ApiResponse<CheckoutSessionResponseDto>.Fail(PaymentMessages.BookingAlreadyPaid);

        if (booking.PaymentStatus == PaymentStatus.Failed ||
            booking.BookingStatus == BookingStatus.Cancelled ||
            booking.BookingStatus == BookingStatus.Failed)
            return ApiResponse<CheckoutSessionResponseDto>.Fail(PaymentMessages.BookingExpiredOrCancelled);

        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == booking.ServiceId);

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(booking.TotalAmount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{service?.Name}",
                            Description = $"Booking on {booking.SlotDate} from {booking.SlotStartTime} to {booking.SlotEndTime}"
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            SuccessUrl = $"{_config["App:ClientUrl"]}/customer/booking/success?bookingId={booking.Id}",
            CancelUrl = $"{_config["App:ClientUrl"]}/customer/checkout/{booking.ServiceId}?cancelled=true",
            Metadata = new Dictionary<string, string>
            {
                { "booking_id", booking.Id.ToString() }
            }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(options);

        return ApiResponse<CheckoutSessionResponseDto>.SuccessResponse(
            PaymentMessages.CheckoutSessionCreated,
            new CheckoutSessionResponseDto
            {
                Url = session.Url,
                SessionId = session.Id
            });
    }

    public async Task<ApiResponse<bool>> HandleWebhookAsync(string json, string stripeSignature)
    {
        try
        {
            var webhookSecret = _config["Stripe:WebhookSecret"];

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret,
                throwOnApiVersionMismatch: false
            );

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    var successSession = stripeEvent.Data.Object as Session;
                    if (successSession != null)
                        await HandlePaymentSuccessAsync(successSession);
                    break;

                case EventTypes.CheckoutSessionExpired:
                    var expiredSession = stripeEvent.Data.Object as Session;
                    if (expiredSession != null)
                        await HandlePaymentExpiredAsync(expiredSession);
                    break;

                case EventTypes.ChargeRefunded:
                    var charge = stripeEvent.Data.Object as Charge;
                    if (charge != null)
                        await HandleRefundAsync(charge);
                    break;

                default:
                    break;
            }

            return ApiResponse<bool>.SuccessResponse(PaymentMessages.WebhookHandled, true);
        }
        catch (StripeException ex)
        {
            return ApiResponse<bool>.Fail($"Stripe error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ExpireTimedOutBookingsAsync()
    {
        var expiredBookings = await _context.Bookings
            .Where(b => b.PaymentStatus == PaymentStatus.Pending
                     && b.BookingStatus == BookingStatus.Pending
                     && b.CreatedAt.AddMinutes(5) < DateTime.UtcNow
                     && b.PaymentMethod != Domain.Enums.PaymentMethod.Cash)
            .ToListAsync();

        if (!expiredBookings.Any())
            return ApiResponse<bool>.SuccessResponse(PaymentMessages.NoExpiredBookings, false);

        foreach (var booking in expiredBookings)
        {
            booking.BookingStatus = BookingStatus.Failed;
            booking.PaymentStatus = PaymentStatus.Failed;
            booking.PartnerId = null;
            booking.ModifiedAt = DateTime.UtcNow;

            _logger.LogInformation("Payment refunded for booking {BookingId}", booking.Id);
            var exists = await _context.Payments.AnyAsync(p => p.BookingId == booking.Id);
            if (!exists)
            {
                _context.Payments.Add(new Payment
                {
                    BookingId = booking.Id,
                    TransactionId = $"REFUND_{booking.Id}_{DateTime.UtcNow.Ticks}",
                    Amount = booking.TotalAmount,
                    PaymentMethod = Domain.Enums.PaymentMethod.DebitCard,
                    PaymentStatus = PaymentStatus.Failed,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse($"{expiredBookings.Count} booking(s) expired.", true);
    }

    private async Task HandlePaymentSuccessAsync(Session session)
    {
        var bookingId = int.Parse(session.Metadata["booking_id"]);
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking is null) return;

        if (booking.BookingStatus == BookingStatus.Cancelled ||
            booking.PaymentStatus == PaymentStatus.Failed)
        {
            await InitiateRefundAsync(session.PaymentIntentId, bookingId, booking.TotalAmount);
            return;
        }

        booking.PaymentStatus = PaymentStatus.Paid;
        booking.BookingStatus = BookingStatus.Pending;
        booking.ModifiedAt = DateTime.UtcNow;

        var alreadyExists = await _context.Payments.AnyAsync(p => p.BookingId == bookingId);
        if (!alreadyExists)
        {
            _context.Payments.Add(new Payment
            {
                BookingId = bookingId,
                TransactionId = session.PaymentIntentId ?? session.Id,
                Amount = booking.TotalAmount,
                PaymentMethod = Domain.Enums.PaymentMethod.DebitCard,
                PaymentStatus = PaymentStatus.Paid,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        await NotifyAdminsAsync(booking, booking.CustomerId);
    }

    private async Task HandlePaymentExpiredAsync(Session session)
    {
        if (!session.Metadata.TryGetValue("booking_id", out var bookingIdStr)) return;
        var bookingId = int.Parse(bookingIdStr);

        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking is null) return;

        if (booking.PaymentStatus != PaymentStatus.Pending) return;

        booking.PaymentStatus = PaymentStatus.Failed;
        booking.BookingStatus = BookingStatus.Failed;
        booking.ModifiedAt = DateTime.UtcNow;

        var alreadyExists = await _context.Payments.AnyAsync(p => p.BookingId == bookingId);
        if (!alreadyExists)
        {
            _context.Payments.Add(new Payment
            {
                BookingId = bookingId,
                TransactionId = $"EXPIRED_{session.Id}",
                Amount = booking.TotalAmount,
                PaymentMethod = Domain.Enums.PaymentMethod.DebitCard,
                PaymentStatus = PaymentStatus.Failed,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task InitiateRefundAsync(string? paymentIntentId, int bookingId, decimal amount)
    {
        if (string.IsNullOrEmpty(paymentIntentId))
        {
            return;
        }

        try
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Reason = RefundReasons.RequestedByCustomer,
                Metadata = new Dictionary<string, string>
                {
                    { "booking_id", bookingId.ToString() },
                    { "reason", "Booking expired before payment" }
                }
            };

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(refundOptions);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment != null)
            {
                payment.PaymentStatus = PaymentStatus.Failed;
                payment.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Payments.Add(new Payment
                {
                    BookingId = bookingId,
                    TransactionId = paymentIntentId,
                    Amount = amount,
                    PaymentMethod = Domain.Enums.PaymentMethod.DebitCard,
                    PaymentStatus = PaymentStatus.Failed,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Refund failed for booking {BookingId}", bookingId);
        }
    }

    private async Task HandleRefundAsync(Charge charge)
    {
        var paymentIntentId = charge.PaymentIntentId;

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);

        if (payment is null)
        {
            return;
        }

        payment.PaymentStatus = PaymentStatus.Failed;
        payment.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<ApiResponse<BookingSuccessResponseDto>> GetBookingSuccessDetailsAsync(int bookingId, int userId)
    {
        var booking = await _context.Bookings
       .AsNoTracking()
       .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

        if (booking is null)
            return ApiResponse<BookingSuccessResponseDto>.Fail(PaymentMessages.BookingNotFound);

        if (booking.PaymentStatus != PaymentStatus.Paid && booking.PaymentMethod != Domain.Enums.PaymentMethod.Cash)
        {
            var failedPayment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            var reason = failedPayment?.TransactionId?.StartsWith("EXPIRED_") == true
                ? PaymentMessages.PaymentExpired
                : PaymentMessages.PaymentRefunded;

            return ApiResponse<BookingSuccessResponseDto>.Fail(
                PaymentMessages.PaymentNotCompleted,
                new BookingSuccessResponseDto
                {
                    ServiceId = booking.ServiceId,
                    FailureReason = reason
                });
        }

        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == booking.ServiceId);

        var category = service != null
            ? await _context.SubCategories
                .AsNoTracking()
                .Where(sc => sc.Id == service.SubCategoryId)
                .Select(sc => sc.Name)
                .FirstOrDefaultAsync()
            : null;

        var address = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == booking.AddressId);

        var location = address != null
            ? $"{address.HouseFlatNo}, {address.Landmark}, {address.DisplayName}"
            : "Address not available";

        string? partnerName = null;
        if (booking.PartnerId.HasValue)
        {
            partnerName = await _context.ServicePartners
                .AsNoTracking()
                .Where(p => p.Id == booking.PartnerId.Value)
                .Select(p => p.FullName)
                .FirstOrDefaultAsync();
        }

        string? partnerImage = null;
        if (booking.PartnerId.HasValue)
        {
            partnerImage = await _context.ServicePartners
                .AsNoTracking()
                .Where(p => p.Id == booking.PartnerId.Value)
                .Select(p => p.ProfileImage)
                .FirstOrDefaultAsync();
        }

        string? partnerMobileNumber = null;
        if (booking.PartnerId.HasValue)
        {
            partnerMobileNumber = await _context.ServicePartners
                .AsNoTracking()
                .Where(p => p.Id == booking.PartnerId.Value)
                .Select(p => p.MobileNumber)
                .FirstOrDefaultAsync();
        }

        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        if (payment is null)
            return ApiResponse<BookingSuccessResponseDto>.Fail(PaymentMessages.PaymentNotFound);

        if (booking.PaymentMethod == Domain.Enums.PaymentMethod.DebitCard && payment.PaymentStatus != PaymentStatus.Paid)
        {
            var reason = payment.TransactionId?.StartsWith("EXPIRED_") == true
                ? PaymentMessages.PaymentExpired
                : PaymentMessages.PaymentRefunded;

            return ApiResponse<BookingSuccessResponseDto>.Fail(
                PaymentMessages.PaymentNotCompleted,
                new BookingSuccessResponseDto
                {
                    ServiceId = booking.ServiceId,
                    FailureReason = reason
                });
        }

        var coupon = _context.coupons.FirstOrDefault(c => c.Id == booking.CouponId);
        string? couponCode = null;

        if (coupon != null)
        {
            couponCode = coupon.CouponCode;
        }

        return ApiResponse<BookingSuccessResponseDto>.SuccessResponse(
            PaymentMessages.BookingSuccessFetched,
            new BookingSuccessResponseDto
            {
                ServiceId = booking.ServiceId,
                BookingId = booking.Id,
                ServiceName = service?.Name ?? "Service",
                ServiceCategory = category ?? "HomeCare Service",
                DurationMinutes = service?.DurationMin ?? 60,
                AmountPaid = booking.TotalAmount,
                Location = location,
                SlotDate = booking.SlotDate.ToString("yyyy-MM-dd"),
                SlotStartTime = booking.SlotStartTime.ToString(@"hh\:mm"),
                PartnerAssigned = partnerName != null,
                PartnerName = partnerName,
                PartnerImage = partnerImage,
                PartnerMobileNumber = partnerMobileNumber,
                InvoicePath = payment.InvoicePath,
                couponCode = couponCode
            });
    }

    public async Task<ApiResponse<bool>> AutoCompleteBookingsAsync()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var completedCount = await _context.Bookings
            .Where(b =>
                (b.BookingStatus == BookingStatus.Pending ||
                 b.BookingStatus == BookingStatus.InProgress) &&
                (b.SlotDate < today ||
                (b.SlotDate == today && b.SlotEndTime <= nowTime)) &&
                (
                    (b.PaymentMethod == Domain.Enums.PaymentMethod.DebitCard &&
                     b.PaymentStatus == PaymentStatus.Paid)
                    ||
                    (b.PaymentMethod == Domain.Enums.PaymentMethod.Cash &&
                     b.PaymentStatus == PaymentStatus.Pending)
                )
            )
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.BookingStatus, BookingStatus.Completed)
                .SetProperty(b => b.PaymentStatus,
                    b => b.PaymentMethod == Domain.Enums.PaymentMethod.Cash
                        ? PaymentStatus.Paid
                        : b.PaymentStatus
                )
                .SetProperty(b => b.ModifiedAt, DateTime.UtcNow)
            );

        await _context.Payments
            .Where(p => _context.Bookings.Any(b =>
                b.Id == p.BookingId &&
                b.BookingStatus == BookingStatus.Completed &&
                b.PaymentMethod == Domain.Enums.PaymentMethod.Cash
            ))
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.PaymentStatus, PaymentStatus.Paid)
                .SetProperty(p => p.ModifiedAt, DateTime.UtcNow)
            );

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(
            $"{completedCount} booking(s) marked as completed.", true);
    }
    public async Task<ApiResponse<bool>> AutoStartBookingsAsync()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var startedCount = await _context.Bookings
            .Where(b =>
                b.BookingStatus == BookingStatus.Pending &&
                b.SlotDate == today &&
                b.SlotStartTime <= nowTime &&
                b.SlotEndTime > nowTime &&
                (
                    (b.PaymentMethod == Domain.Enums.PaymentMethod.DebitCard &&
                     b.PaymentStatus == PaymentStatus.Paid)
                    ||
                    (b.PaymentMethod == Domain.Enums.PaymentMethod.Cash &&
                     b.PaymentStatus == PaymentStatus.Pending)
                )
            )
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.BookingStatus, BookingStatus.InProgress)
                .SetProperty(b => b.ModifiedAt, DateTime.UtcNow)
            );

        return ApiResponse<bool>.SuccessResponse(
            $"{startedCount} booking(s) started.", true);
    }

    public async Task<ApiResponse<bool>> RefundBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return ApiResponse<bool>.Fail("Booking not found.");

        if (booking.PaymentMethod != Domain.Enums.PaymentMethod.DebitCard ||
            booking.PaymentStatus != PaymentStatus.Paid)
            return ApiResponse<bool>.SuccessResponse("No refund needed.", false);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        if (payment == null)
            return ApiResponse<bool>.Fail("Payment record not found.");

        if (payment.PaymentStatus == PaymentStatus.Failed)
            return ApiResponse<bool>.SuccessResponse("Already refunded.", false);

        await InitiateRefundAsync(payment.TransactionId, bookingId, booking.TotalAmount);

        booking.PaymentStatus = PaymentStatus.Failed;
        booking.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Refund initiated successfully.", true);
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
