using Homecare.Application.Constants.Payments;
using Homecare.Application.Interfaces;
using Homecare.Application.Interfaces.Payments;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace Homecare.Application.Services.Payments;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinary;

    public InvoiceService(AppDbContext context, ICloudinaryService cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }

    public async Task<string> GetInvoicePathAsync(int bookingId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId)
            ?? throw new KeyNotFoundException(PaymentMessages.PaymentNotFoundForBooking);

        var cloudinaryUrl = await GenerateAsync(bookingId);

        await _context.Entry(payment).ReloadAsync();

        if (string.IsNullOrWhiteSpace(payment.InvoicePath))
            throw new FileNotFoundException(PaymentMessages.InvoiceNotGenerated);

        return payment.InvoicePath;
    }

    public async Task<string> GenerateAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Address)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new Exception($"{PaymentMessages.BookingNotFound} (ID: {bookingId})");

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == booking.ServiceId)
            ?? throw new Exception(PaymentMessages.ServiceNotFound);

        var subCategoryName = await _context.SubCategories
            .Where(sc => sc.Id == service.SubCategoryId)
            .Select(sc => sc.Name)
            .FirstOrDefaultAsync() ?? "Service";

        string? partnerName = null;
        string? partnerRole = null;
        string? partnerMobileNumber = null;

        if (booking.PartnerId.HasValue)
        {
            var partner = await _context.ServicePartners
                .Where(p => p.Id == booking.PartnerId.Value)
                .Select(p => new { p.FullName, p.ServiceTypeId, p.MobileNumber })
                .FirstOrDefaultAsync();

            partnerName = partner?.FullName;

            if (partner != null)
            {
                partnerRole = await _context.ServiceTypes
                    .Where(st => st.Id == partner.ServiceTypeId)
                    .Select(st => st.Name)
                    .FirstOrDefaultAsync();

                partnerMobileNumber = partner.MobileNumber;
            }
        }

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId)
            ?? throw new KeyNotFoundException(PaymentMessages.PaymentNotFoundForBooking);

        if (payment.PaymentMethod == PaymentMethod.DebitCard &&
            payment.PaymentStatus != PaymentStatus.Paid)
            throw new Exception(PaymentMessages.PaymentNotCompleted);

        decimal walletDiscount = booking.WalletDiscountAmount;
        decimal refereeDiscount = 0;

        bool hasReferral = await _context.ReferralUses
            .AnyAsync(r => r.RefereeId == booking.CustomerId);

        if (hasReferral && booking.CouponId == null)
            refereeDiscount = booking.DiscountAmount;

        decimal couponDiscount = booking.DiscountAmount - refereeDiscount;

        string? couponCode = null;
        if (booking.CouponId.HasValue)
        {
            couponCode = await _context.coupons
                .Where(c => c.Id == booking.CouponId.Value)
                .Select(c => c.CouponCode)
                .FirstOrDefaultAsync();
        }

        var doc = new InvoiceDocument(
            booking, service, payment,
            booking.Address, _context,
            partnerName, partnerRole, partnerMobileNumber,
            subCategoryName,
            couponCode,
            couponDiscount,
            refereeDiscount,
            walletDiscount);

        byte[] pdfBytes = doc.GeneratePdf();

        var fileName = $"INV-{bookingId:D7}.pdf";
        var cloudinaryUrl = await _cloudinary.UploadRawFileAsync(
            pdfBytes, fileName, "invoices");

        payment.InvoicePath = cloudinaryUrl;
        payment.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return cloudinaryUrl;
    }
}