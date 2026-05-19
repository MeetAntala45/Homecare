using Homecare.Application.Constants.Payments;
using Homecare.Application.Interfaces.Payments;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace Homecare.Application.Services.Payments;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public InvoiceService(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<string> GetInvoicePathAsync(int bookingId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId)
            ?? throw new KeyNotFoundException(PaymentMessages.PaymentNotFoundForBooking);

        await GenerateAsync(bookingId);

        await _context.Entry(payment).ReloadAsync();

        var fullPath = Path.Combine(_env.WebRootPath, payment.InvoicePath!);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException(PaymentMessages.InvoiceNotGenerated);

        return fullPath;
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

        var folder = Path.Combine(_env.WebRootPath, "invoices");
        Directory.CreateDirectory(folder);

        var fileName = $"INV-{bookingId:D7}.pdf";
        var filePath = Path.Combine(folder, fileName);
        var relativePath = $"invoices/{fileName}";

        var doc = new InvoiceDocument(
            booking, service, payment,
            booking.Address, _context, partnerName, partnerRole, partnerMobileNumber, subCategoryName);

        doc.GeneratePdf(filePath);

        payment.InvoicePath = relativePath;
        payment.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return relativePath;
    }
}