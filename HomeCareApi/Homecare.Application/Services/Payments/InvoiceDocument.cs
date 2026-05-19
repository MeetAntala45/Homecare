using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using IContainer = QuestPDF.Infrastructure.IContainer;
 
namespace Homecare.Application.Services.Payments;
 
public class InvoiceDocument : IDocument
{
    private readonly Booking _booking;
    private readonly Service _service;
    private readonly Payment _payment;
    private readonly Address _address;
    private readonly string? _partnerName;
    private readonly string? _partnerRole;
    private readonly string? _partnerMobileNumber;
    
    private readonly string _subCategoryName;
 
    private static readonly string Primary = "#4F46E5";
    private static readonly string DarkText = "#1E1B4B";
    private static readonly string GrayText = "#6B7280";
    private static readonly string LightBg = "#F5F5FF";
    private static readonly string Border = "#E5E7EB";
    private static readonly string Green = "#16A34A";
 
    private readonly AppDbContext _context;
 
    public InvoiceDocument(
        Booking booking,
        Service service,
        Payment payment,
        Address address,
        AppDbContext context,
        string? partnerName,
        string? partnerRole,
        string? partnerMobileNumber,
        string subCategoryName)
    {
        _booking = booking;
        _service = service;
        _payment = payment;
        _address = address;
        _context = context;
        _partnerName = partnerName;
        _partnerRole = partnerRole;
        _partnerMobileNumber = partnerMobileNumber;
        _subCategoryName = subCategoryName;
    }
 
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
 
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(15, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(9).FontColor(DarkText));
 
            page.Content().Column(col =>
            {
                col.Item().Element(ComposeHeader);
                col.Item().PaddingTop(8).Element(ComposeBillAndMeta);
                col.Item().PaddingTop(12).Element(ComposeServiceTable);
                col.Item().PaddingTop(4).Element(ComposeTotals);
                col.Item().PaddingTop(12).Element(ComposeBookingDetails);
            });
 
            page.Footer().Element(ComposeFooter);
        });
    }
 
    void ComposeHeader(IContainer c)
{
    c.Column(col =>
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().AlignMiddle().Text(text =>
            {
                text.Span("H").FontSize(22).Bold().FontColor("#1E1B4B");
                text.Span("o").FontSize(22).Bold().FontColor(Primary);
                text.Span("mecare").FontSize(22).Bold().FontColor("#1E1B4B");
            });
 
            row.RelativeItem().AlignRight().AlignMiddle().Text("INVOICE")
                .FontSize(28).Bold().FontColor(Primary);
        });
 
        col.Item().PaddingTop(6)
           .BorderBottom(1.5f).BorderColor(Primary)
           .PaddingBottom(0);
    });
}
 
    void ComposeBillAndMeta(IContainer c)
    {
        var customer = _booking.Customer;
        var addressLine = $"{_address.HouseFlatNo}, {_address.Landmark}, {_address.DisplayName}";
 
        c.Row(row =>
        {
            row.RelativeItem().Border(0.5f).BorderColor(Border)
               .Background(LightBg).Padding(10).Column(inner =>
               {
                   inner.Item().Text("BILL TO").FontSize(7).FontColor(GrayText);
                   inner.Item().PaddingTop(3).Text(customer?.Name ?? "Customer")
                       .Bold().FontSize(11);
                   inner.Item().PaddingTop(2).Text(customer?.Email ?? "")
                       .FontColor(GrayText);
                   inner.Item().PaddingTop(3).Text(addressLine)
                       .FontSize(8)
                       .FontColor(GrayText);
               });
 
            row.ConstantItem(20);
 
            row.RelativeItem().Column(inner =>
            {
                MetaRow(inner, "Invoice ID", $"INV-{_booking.Id:D7}");
                MetaRow(inner, "Booking ID", $"BKG-{_booking.Id:D5}");
                MetaRow(inner, "Invoice Date", _payment.CreatedAt.ToString("dd MMM yyyy"));
                MetaRow(inner, "Transaction ID", _payment.TransactionId);
                MetaRow(inner, "Payment", _booking.PaymentMethod.ToString());
                inner.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Status").FontSize(8).FontColor(GrayText);
 
                    var isCash = _booking.PaymentMethod == PaymentMethod.Cash;
                    var statusText  = isCash
                        ? (_booking.BookingStatus == BookingStatus.Completed ? "Paid" : "Pending")
                        : "Paid";
                    var statusColor = statusText == "Paid" ? Green : "#F59E0B";
 
                    r.RelativeItem().AlignRight()
                        .Text(statusText).Bold().FontColor(statusColor);
                });
            });
        });
    }
 
    void MetaRow(ColumnDescriptor col, string label, string value)
{
    col.Item().PaddingTop(5).Row(r =>
    {
        r.ConstantItem(28, Unit.Millimetre).Text(label)
            .FontSize(8).FontColor(GrayText);
        r.RelativeItem().AlignRight().Text(value).Bold().FontSize(8);
    });
}
 
    void ComposeServiceTable(IContainer c)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2);                    
                cols.ConstantColumn(22, Unit.Millimetre);
                cols.RelativeColumn(2);
                cols.ConstantColumn(28, Unit.Millimetre);
            });
 
            static IContainer HeaderCell(IContainer cell) =>
                cell.Background("#4F46E5").Padding(6);
 
            table.Header(header =>
            {
                header.Cell().Element(HeaderCell)
                    .Text("SERVICE").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Element(HeaderCell)
                    .Text("DURATION").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Element(HeaderCell)
                    .Text("PARTNER").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Element(HeaderCell).AlignRight()
                    .Text("AMOUNT").FontColor(Colors.White).Bold().FontSize(8);
            });
 
            static IContainer DataCell(IContainer cell) =>
                cell.BorderBottom(0.3f).BorderColor("#E5E7EB").Padding(6);
 
 
            var normalisedServiceName = System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(_service.Name.Trim().ToLower());
 
            table.Cell().Element(DataCell).Column(inner =>
            {
                inner.Item().Text(normalisedServiceName).Bold();
                inner.Item().Text(_subCategoryName).FontColor(GrayText).FontSize(8);
            });
 
            table.Cell().Element(DataCell).AlignCenter()
                .Text($"{_service.DurationMin} Min").FontColor(DarkText);
 
            table.Cell().Element(DataCell).Column(inner =>
            {
                if (_partnerName != null)
                {
                    inner.Item().Text(text =>
                    {
                        text.Span(_partnerName)
                            .Bold()
                            .FontSize(10)
                            .FontColor(DarkText);

                        if (!string.IsNullOrEmpty(_partnerMobileNumber))
                        {
                            text.Span("  ");

                            text.Span($"({_partnerMobileNumber})")
                                .FontSize(9)
                                .FontColor(GrayText);
                        }
                    });

                    inner.Item().Text(_partnerRole ?? "Service Partner")
                        .FontColor(GrayText).FontSize(8);
                }
                else
                {
                    inner.Item().Text("Not yet assigned").FontColor(GrayText).Italic();
                }
            });
 
            table.Cell().Element(DataCell).AlignRight()
                .Text($"${_booking.ServicePrice:F2}").Bold();
        });
    }
 
    void ComposeTotals(IContainer c)
    {
        c.AlignRight().Width(90, Unit.Millimetre).Column(col =>
        {
            TotalRow(col, "Service Price",  $"${_booking.ServicePrice:F2}");
            TotalRow(col, $"Tax ({_booking.TaxPct:F0}%)",  $"${_booking.TaxAmount:F2}");
 
            var coupon = _context.coupons.FirstOrDefault(c => c.Id == _booking.CouponId);
            if (_booking.DiscountAmount > 0)
                TotalRow(col, $"Discount ({coupon!.CouponCode})" ,  $"-${_booking.DiscountAmount:F2}",
                    valueColor: Green);
 
            col.Item().PaddingTop(4)
               .BorderTop(1.2f).BorderColor(Primary)
               .PaddingTop(4)
               .Row(r =>
               {
                   r.RelativeItem().Text("TOTAL  AMOUNT").Bold().FontColor(Primary);
                   r.RelativeItem().AlignRight()
                       .Text($"${_booking.TotalAmount:F2}").Bold().FontColor(Primary);
               });
        });
    }
 
    void TotalRow(ColumnDescriptor col, string label, string value,
        string? valueColor = null)
    {
        col.Item().PaddingTop(3).Row(r =>
        {
            r.RelativeItem().Text(label).FontColor(GrayText);
            r.RelativeItem().AlignRight().Text(value)
                .FontColor(valueColor ?? DarkText);
        });
    }
 
    void ComposeBookingDetails(IContainer c)
    {
        var slotDisplay = $"{_booking.SlotDate:dd MMM yyyy}  •  " +
                          $"{_booking.SlotStartTime:hh\\:mm tt}";
        var addressFull = $"{_address.HouseFlatNo}, {_address.Landmark}, " +
                          $"{_address.DisplayName}";
 
        c.Column(col =>
        {
            col.Item().PaddingTop(6).Text("Booking Details").Bold().FontSize(11);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(40, Unit.Millimetre);
                    cols.RelativeColumn();
                });
 
                DetailRow(table, "Service Date & Time", slotDisplay);
                DetailRow(table, "Service Address", addressFull);
            });
        });
    }
 
    void DetailRow(TableDescriptor table, string label, string value)
    {
        static IContainer Cell(IContainer c) =>
            c.Background("#F5F5FF").BorderBottom(0.3f).BorderColor("#E5E7EB")
             .Padding(8);
 
        table.Cell().Element(Cell).Text(label).FontColor(GrayText).FontSize(8);
        table.Cell().Element(Cell).Text(value).Bold();
    }
 
    void ComposeFooter(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().BorderTop(0.5f).BorderColor(Border).PaddingTop(6).Row(r =>
            {
                r.RelativeItem().AlignRight().Text("Thank you for choosing HomeCare!")
                    .FontSize(8).FontColor(GrayText);
            });
        });
    }
}