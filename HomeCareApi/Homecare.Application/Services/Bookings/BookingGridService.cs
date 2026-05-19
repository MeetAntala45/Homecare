using Homecare.Application.Constants;
using Homecare.Application.Constants.Bookings;
using Homecare.Application.DTOs.BookingManagement;
using Homecare.Application.DTOs.Bookings;
using Homecare.Application.Interfaces.Bookings;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Bookings;

public class BookingGridService : IBookingGridService
{
    private readonly AppDbContext _context;

    public BookingGridService(AppDbContext context)
    {
        _context = context;
    }

    private sealed class CustomerPaymentGroup
    {
        public int CustomerId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int TotalBookedServices { get; set; }
        public decimal TotalBookingAmount { get; set; }
        public string CustomerName { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string AddressDisplay { get; set; } = null!;
    }

    public async Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypesAsync()
    {
        var items = await _context.ServiceTypes
            .Where(st => !st.IsDeleted)
            .OrderBy(st => st.Name)
            .Select(st => new DropdownOptionDto { Label = st.Name, Value = st.Name })
            .AsNoTracking()
            .ToListAsync();

        return ApiResponse<List<DropdownOptionDto>>.SuccessResponse("Service types fetched.", items);
    }

    public async Task<ApiResponse<CustomerBookingSummaryPagedDto>> GetCustomerBookingSummariesAsync(
        BookingGridRequestDto request)
    {
        DateOnly? fromDate = null;
        if (!string.IsNullOrWhiteSpace(request.FromDate) &&
            DateOnly.TryParse(request.FromDate, out var pd))
            fromDate = pd;

        TimeOnly? fromTime = null;
        if (!string.IsNullOrWhiteSpace(request.FromTime) &&
            TimeOnly.TryParse(request.FromTime, out var pt))
            fromTime = pt;

        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Address)
            .Where(b => !b.IsDeleted && b.BookingStatus != BookingStatus.Failed)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(b => b.SlotDate == fromDate.Value);

        if (fromTime.HasValue)
            query = query.Where(b => b.SlotStartTime == fromTime.Value);

        if (request.PaymentMethod.HasValue)
            query = query.Where(b => (int)b.PaymentMethod == request.PaymentMethod.Value);

        if (request.BookingStatus.HasValue)
            query = query.Where(b => (int)b.BookingStatus == request.BookingStatus.Value);

        if (!string.IsNullOrWhiteSpace(request.ServiceType))
            query = query.Where(b =>
                _context.Services
                    .Where(s => s.Id == b.ServiceId)
                    .Join(_context.SubCategories,
                        s => s.SubCategoryId, sc => sc.Id,
                        (s, sc) => sc.CategoryId)
                    .Join(_context.Categories,
                        cid => cid, c => c.Id,
                        (cid, c) => c.ServiceTypeId)
                    .Join(_context.ServiceTypes,
                        stid => stid, st => st.Id,
                        (stid, st) => st.Name)
                    .Any(name => name == request.ServiceType));

        var grouped = query
            .GroupBy(b => new { b.CustomerId, b.PaymentMethod })
            .Select(g => new CustomerPaymentGroup
            {
                CustomerId = g.Key.CustomerId,
                PaymentMethod = g.Key.PaymentMethod,
                TotalBookedServices = g.Count(),
                TotalBookingAmount = request.BookingStatus.HasValue
                    ? g.Sum(b => b.TotalAmount)
                    : (g.Key.PaymentMethod == PaymentMethod.Cash
                        ? g.Where(b => b.BookingStatus == BookingStatus.Completed)
                            .Sum(b => b.TotalAmount)
                        : g.Where(b => b.BookingStatus == BookingStatus.Completed ||
                                    b.BookingStatus == BookingStatus.Pending ||
                                    b.BookingStatus == BookingStatus.InProgress)
                            .Sum(b => b.TotalAmount)),
                CustomerName = g.First().Customer.Name,
                MobileNumber = g.First().Customer.MobileNumber ?? "—",
                Email = g.First().Customer.Email,
                AddressDisplay = g
                    .OrderByDescending(b => b.SlotDate)
                    .Select(b => b.Address.HouseFlatNo + ", " + b.Address.Landmark + ", " + b.Address.DisplayName)
                    .FirstOrDefault() ?? "-"
            });

        if (request.MinBookings.HasValue)
            grouped = grouped.Where(g => g.TotalBookedServices >= request.MinBookings.Value);

        if (request.MaxBookings.HasValue)
            grouped = grouped.Where(g => g.TotalBookedServices <= request.MaxBookings.Value);

        if (request.MinAmount.HasValue)
            grouped = grouped.Where(g => g.TotalBookingAmount >= request.MinAmount.Value);

        if (request.MaxAmount.HasValue)
            grouped = grouped.Where(g => g.TotalBookingAmount <= request.MaxAmount.Value);

        grouped = grouped.Where(g => g.TotalBookedServices > 0);

        var rangeQuery = query
            .GroupBy(b => new { b.CustomerId, b.PaymentMethod })
            .Select(g => new
            {
                TotalBookedServices = g.Count(),
                TotalBookingAmount = g.Key.PaymentMethod == PaymentMethod.Cash
                    ? g.Where(b => b.BookingStatus == BookingStatus.Completed)
                       .Sum(b => b.TotalAmount)
                    : g.Where(b => b.BookingStatus == BookingStatus.Completed ||
                                   b.BookingStatus == BookingStatus.Pending ||
                                   b.BookingStatus == BookingStatus.InProgress)
                       .Sum(b => b.TotalAmount)
            })
            .Where(g => g.TotalBookedServices > 0);

        var ranges = await rangeQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                MinBookedServices = g.Min(x => x.TotalBookedServices),
                MaxBookedServices = g.Max(x => x.TotalBookedServices),
                MinAmount = g.Min(x => (decimal?)x.TotalBookingAmount) ?? 0,
                MaxAmount = g.Max(x => (decimal?)x.TotalBookingAmount) ?? 0,
            })
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var normalisedName = request.UserName
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            grouped = grouped.Where(x =>
                normalisedName.All(term =>
                    x.CustomerName.ToLower().Contains(term)
                )
            );
        }

        var totalCount = await grouped.CountAsync();

        var sorted = (request.SortBy?.ToLower().Trim(), request.SortOrder?.ToLower().Trim()) switch
        {
            ("totalbookedservices", "desc") => grouped
                .OrderByDescending(g => g.TotalBookedServices)
                .ThenBy(g => g.CustomerId).ThenBy(g => g.PaymentMethod),
            ("totalbookedservices", _) => grouped
                .OrderBy(g => g.TotalBookedServices)
                .ThenBy(g => g.CustomerId).ThenBy(g => g.PaymentMethod),
            ("totalbookingamount", "desc") => grouped
                .OrderByDescending(g => g.TotalBookingAmount)
                .ThenBy(g => g.CustomerId).ThenBy(g => g.PaymentMethod),
            ("totalbookingamount", _) => grouped
                .OrderBy(g => g.TotalBookingAmount)
                .ThenBy(g => g.CustomerId).ThenBy(g => g.PaymentMethod),
            ("customername", "desc") => grouped
                .OrderByDescending(g => g.CustomerName)
                .ThenBy(g => g.CustomerId).ThenBy(g => g.PaymentMethod),
            _ => grouped
                .OrderBy(g => g.CustomerName)
                .ThenBy(g => g.CustomerId).ThenBy(g => g.PaymentMethod),
        };

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var items = await sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(g => new CustomerBookingSummaryDto
        {
            CustomerId = g.CustomerId,
            CustomerName = g.CustomerName,
            MobileNumber = g.MobileNumber,
            Email = g.Email,
            Address = g.AddressDisplay,
            TotalBookedServices = g.TotalBookedServices,
            TotalBookingAmount = g.TotalBookingAmount,
            PaymentMethod = g.PaymentMethod.ToString(),
            PaymentMethodValue = (int)g.PaymentMethod
        }).ToList();

        return ApiResponse<CustomerBookingSummaryPagedDto>.SuccessResponse(
            BookingMessages.FetchedSuccessfully,
            new CustomerBookingSummaryPagedDto
            {
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                MinBookedServices = ranges?.MinBookedServices ?? 0,
                MaxBookedServices = ranges?.MaxBookedServices ?? 0,
                MinAmount = ranges?.MinAmount ?? 0,
                MaxAmount = ranges?.MaxAmount ?? 0,
            });
    }

    public async Task<ApiResponse<bool>> DeleteCustomerBookingsAsync(int customerId, int paymentMethodValue)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer is null)
            return ApiResponse<bool>.Fail(BookingMessages.CustomerNotFound);

        var bookings = await _context.Bookings
            .Where(b => b.CustomerId == customerId && !b.IsDeleted && (int)b.PaymentMethod == paymentMethodValue && b.BookingStatus == BookingStatus.Cancelled)
            .ToListAsync();

        foreach (var booking in bookings)
        {
            if (booking.Payment != null)
            {
                booking.Payment.IsDeleted = true;
                booking.Payment.ModifiedAt = DateTime.UtcNow;
            }

            booking.IsDeleted = true;
            booking.ModifiedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(BookingMessages.CustomerDeletedSuccessfully, true);
    }

    public async Task<ApiResponse<CustomerBookingDetailPagedDto>> GetCustomerBookingDetailsAsync(
        CustomerBookingDetailRequestDto request)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 5 : request.PageSize;

        DateOnly? fromDate = null;
        if (!string.IsNullOrWhiteSpace(request.FromDate) &&
            DateOnly.TryParse(request.FromDate, out var pd))
            fromDate = pd;

        TimeOnly? fromTime = null;
        if (!string.IsNullOrWhiteSpace(request.FromTime) &&
            TimeOnly.TryParse(request.FromTime, out var pt))
            fromTime = pt;

        var query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.CustomerId == request.CustomerId
                     && (int)b.PaymentMethod == request.PaymentMethod
                     && !b.IsDeleted
                     && b.BookingStatus != BookingStatus.Failed);

        if (fromDate.HasValue)
            query = query.Where(b => b.SlotDate == fromDate.Value);

        if (fromTime.HasValue)
            query = query.Where(b => b.SlotStartTime == fromTime.Value);

        if (request.BookingStatus.HasValue)
            query = query.Where(b => (int)b.BookingStatus == request.BookingStatus.Value);

        if (!string.IsNullOrWhiteSpace(request.ServiceType))
            query = query.Where(b =>
                _context.Services
                    .Where(s => s.Id == b.ServiceId)
                    .Join(_context.SubCategories,
                        s => s.SubCategoryId, sc => sc.Id,
                        (s, sc) => sc.CategoryId)
                    .Join(_context.Categories,
                        cid => cid, c => c.Id,
                        (cid, c) => c.ServiceTypeId)
                    .Join(_context.ServiceTypes,
                        stid => stid, st => st.Id,
                        (stid, st) => st.Name)
                    .Any(name => name == request.ServiceType));

        var totalCount = await query.CountAsync();

        bool isDesc = request.SortOrder?.ToLower() != "asc";
        query = request.SortBy?.ToLower() switch
        {
            "bookingid" => isDesc
                ? query.OrderByDescending(b => b.Id)
                : query.OrderBy(b => b.Id),
            "servicename" => isDesc
                ? query.OrderByDescending(b => _context.Services
                    .Where(s => s.Id == b.ServiceId).Select(s => s.Name).FirstOrDefault())
                : query.OrderBy(b => _context.Services
                    .Where(s => s.Id == b.ServiceId).Select(s => s.Name).FirstOrDefault()),
            "datetime" => isDesc
                ? query.OrderByDescending(b => b.SlotDate).ThenByDescending(b => b.SlotStartTime)
                : query.OrderBy(b => b.SlotDate).ThenBy(b => b.SlotStartTime),
            "bookingstatus" => isDesc
                ? query.OrderByDescending(b => b.BookingStatus)
                : query.OrderBy(b => b.BookingStatus),
            _ => query.OrderByDescending(b => b.Id).ThenByDescending(b => b.Id)
        };

        var bookings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                b.Id,
                b.ServiceId,
                b.SlotDate,
                b.SlotStartTime,
                b.BookingStatus,
                b.PartnerId,
                b.TotalAmount,

                ServiceName = _context.Services
                    .Where(s => s.Id == b.ServiceId)
                    .Select(s => s.Name).FirstOrDefault() ?? "—",

                ServiceType = _context.Services
                    .Where(s => s.Id == b.ServiceId)
                    .Join(_context.SubCategories,
                        s => s.SubCategoryId, sc => sc.Id,
                        (s, sc) => sc.CategoryId)
                    .Join(_context.Categories,
                        cid => cid, c => c.Id,
                        (cid, c) => c.ServiceTypeId)
                    .Join(_context.ServiceTypes,
                        stid => stid, st => st.Id,
                        (stid, st) => st.Name)
                    .FirstOrDefault() ?? "—",

                ExpertName = b.PartnerId != null
                    ? _context.ServicePartners.Where(p => p.Id == b.PartnerId)
                        .Select(p => p.FullName).FirstOrDefault()
                    : null,

                ExpertPhoto = b.PartnerId != null
                    ? _context.ServicePartners.Where(p => p.Id == b.PartnerId)
                        .Select(p => p.ProfileImage).FirstOrDefault()
                    : null,

                IsPartnerDeleted = b.PartnerId != null &&
                    _context.ServicePartners.Where(p => p.Id == b.PartnerId)
                        .Select(p => p.IsDeleted).FirstOrDefault(),

                PartnerPhone = b.PartnerId != null
                    ? _context.ServicePartners.Where(p => p.Id == b.PartnerId)
                        .Select(p => p.MobileNumber).FirstOrDefault()
                    : null
            })
            .ToListAsync();

        var dtos = bookings.Select(b => new CustomerBookingDetailDto
        {
            BookingId = b.Id,
            ServiceId = b.ServiceId,
            ServiceName = b.ServiceName,
            ServiceType = b.ServiceType,
            DateTime = $"{b.SlotDate:dd MMM, yyyy} {b.SlotStartTime:hh\\:mm tt}",
            ExpertName = b.ExpertName,
            ExpertPhoto = b.ExpertPhoto,
            BookingStatus = b.BookingStatus.ToString(),
            BookingStatusValue = (int)b.BookingStatus,
            Amount = b.TotalAmount,
            PartnerId = b.PartnerId,
            IsPartnerDeleted = b.IsPartnerDeleted,
            PartnerPhone = b.PartnerPhone
        }).ToList();

        return ApiResponse<CustomerBookingDetailPagedDto>.SuccessResponse(
            BookingMessages.FetchedSuccessfully,
            new CustomerBookingDetailPagedDto
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
    }

    public async Task<int> GetCustomerPositionAsync(
    int customerId,
    int paymentMethod,
    int pageSize)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Address)
            .Where(b => !b.IsDeleted && b.BookingStatus != BookingStatus.Failed);

        var grouped = query
            .GroupBy(b => new { b.CustomerId, b.PaymentMethod })
            .Select(g => new
            {
                CustomerId = g.Key.CustomerId,
                PaymentMethodValue = (int)g.Key.PaymentMethod,
                CustomerName = g.First().Customer.Name
            });

        var sorted = grouped
            .OrderBy(g => g.CustomerName)
            .ThenBy(g => g.CustomerId)
            .ThenBy(g => g.PaymentMethodValue);

        var list = await sorted.ToListAsync();

        var index = list.FindIndex(g =>
            g.CustomerId == customerId &&
            g.PaymentMethodValue == paymentMethod);

        return index < 0
        ? Math.Max(1, (list.Count / pageSize) + 1)
        : (index / pageSize) + 1;
    }
}