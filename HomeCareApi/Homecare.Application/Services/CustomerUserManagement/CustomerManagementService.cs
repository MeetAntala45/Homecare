using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.CustomerUserManagement;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.CustomerUser;
using Homecare.Application.Hubs;
using Homecare.Application.Interfaces.CustomerUserManagement;
using Homecare.Application.Interfaces.Payments;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace Homecare.Application.Services.CustomerUserManagement;

public class CustomerManagementService : ICustomerManagementService
{
    private readonly AppDbContext _context;
    private readonly IPaymentService _paymentService;

    public CustomerManagementService(AppDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<ApiResponse<FilterPagedResult<CustomerListDto>>> GetCustomerListAsync(
        CustomerListFilterDto filter)
    {
        filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;


        var baseQuery = _context.Customers
        .AsNoTracking()
        .Select(c => new
        {
            Customer = c,

            TotalBookings = _context.Bookings
                .Count(b =>
                    b.CustomerId == c.Id &&
                    b.PaymentStatus != PaymentStatus.Failed),

            PendingBookings = _context.Bookings
                .Count(b =>
                    b.CustomerId == c.Id &&
                    b.PaymentStatus != PaymentStatus.Failed &&
                    b.BookingStatus != BookingStatus.Completed &&
                    b.BookingStatus != BookingStatus.Cancelled)
        });

        var bookingRange = await baseQuery
        .GroupBy(_ => 1)
        .Select(g => new
        {
            Min = g.Min(x => (int?)x.TotalBookings) ?? 0,
            Max = g.Max(x => (int?)x.TotalBookings) ?? 0
        })
        .FirstOrDefaultAsync();

        var query = baseQuery;

        if (!string.IsNullOrEmpty(filter.Status) &&
            Enum.TryParse<UserStatus>(filter.Status, true, out var status))
        {
            query = query.Where(x => x.Customer.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var normalisedName = filter.Name
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                normalisedName.All(term =>
                    x.Customer.Name
                        .ToLower()
                        .Replace(" ", "")
                        .Contains(term)
                )
            );
        }

        if (filter.MinBookings.HasValue)
            query = query.Where(x => x.TotalBookings >= filter.MinBookings.Value);

        if (filter.MaxBookings.HasValue)
            query = query.Where(x => x.TotalBookings <= filter.MaxBookings.Value);

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            var normalisedName = filter.UserName
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                normalisedName.All(term =>
                    x.Customer.Name
                        .ToLower()
                        .Contains(term)
                )
            );
        }

        bool isDesc = filter.SortOrder?.ToLower() == "desc";

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            query = filter.SortBy?.ToLower() switch
            {
                "id" => isDesc ? query.OrderByDescending(x => x.Customer.Id) : query.OrderBy(x => x.Customer.Id),

                "name" => isDesc ? query.OrderByDescending(x => x.Customer.Name) : query.OrderBy(x => x.Customer.Name),

                "email" => isDesc ? query.OrderByDescending(x => x.Customer.Email) : query.OrderBy(x => x.Customer.Email),

                "totalbookings" => isDesc ? query.OrderByDescending(x => x.TotalBookings) : query.OrderBy(x => x.TotalBookings),

                "pendingbookings" => isDesc ? query.OrderByDescending(x => x.PendingBookings) : query.OrderBy(x => x.PendingBookings),

                _ => query.OrderByDescending(x => x.Customer.Id)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.Customer.Id);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new CustomerListDto
            {
                Id = x.Customer.Id,
                Name = x.Customer.Name.ToTitleCase(),
                Email = x.Customer.Email,
                MobileNumber = x.Customer.MobileNumber ?? "-",
                TotalBookings = x.TotalBookings,
                PendingBookings = x.PendingBookings,
                Status = x.Customer.Status.ToString()
            })
            .ToListAsync();

        return ApiResponse<FilterPagedResult<CustomerListDto>>.SuccessResponse(
            CustomerManagementMessages.CustomersFetched,
            new FilterPagedResult<CustomerListDto>
            {
                Data = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Min = bookingRange?.Min ?? 0,
                Max = bookingRange?.Max ?? 0
            });
    }

    public async Task<ApiResponse<string>> BlockCustomerAsync(int customerId, int adminId)
    {
        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.CustomerDoesNotExist);

        if (customer.Status == UserStatus.Blocked)
            return ApiResponse<string>.Fail(CustomerManagementMessages.AlreadyBlocked);

        customer.Status = UserStatus.Blocked;
        customer.ModifiedAt = DateTime.UtcNow;
        customer.ModifiedBy = adminId;

        var tokens = await _context.RefreshTokens
            .Where(r => r.CustomerId == customerId && !r.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.CustomerBlocked);
    }

    public async Task<ApiResponse<string>> AddCustomerAsync(int adminId, AddCustomerDto dto)
    {
        var emailExists = await _context.Customers
            .AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower());

        if (emailExists)
            return ApiResponse<string>.Fail(CustomerManagementMessages.EmailAlreadyRegistered);

        var customer = new Customer
        {
            Name = dto.Name.ToTitleCase(),
            Email = dto.Email.ToLower(),
            MobileNumber = dto.MobileNumber,
            Status = UserStatus.Active,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.CustomerAdded);
    }

    public async Task<ApiResponse<string>> DeleteCustomerAsync(int customerId, int adminId)
    {
        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.CustomerDoesNotExist);

        var bookingExists = _context.Bookings
        .Any(b => b.CustomerId == customerId);

        if (bookingExists)
            return ApiResponse<string>.Fail("Bookings already exist for this customer");

        var tokens = await _context.RefreshTokens
            .Where(r => r.CustomerId == customerId && !r.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.IsRevoked = true;
        customer.Status = UserStatus.Inactive;
        customer.ModifiedAt = DateTime.UtcNow;
        customer.ModifiedBy = adminId;
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.CustomerDeleted);
    }

    public async Task<ApiResponse<CustomerListDto>> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return ApiResponse<CustomerListDto>.Fail(CustomerManagementMessages.CustomerDoesNotExist);

        var customerDto = new CustomerListDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            MobileNumber = customer.MobileNumber ?? "-",
            Status = customer.Status.ToString()

        };

        return ApiResponse<CustomerListDto>.SuccessResponse(CustomerManagementMessages.CustomerDetailsFetched, customerDto);
    }

    public async Task<ApiResponse<string>> UnblockCustomerAsync(int customerId, int adminId)
    {
        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.CustomerDoesNotExist);

        if (customer.Status != UserStatus.Blocked)
            return ApiResponse<string>.Fail(CustomerManagementMessages.NotBlocked);

        customer.Status = UserStatus.Active;
        customer.ModifiedAt = DateTime.UtcNow;
        customer.ModifiedBy = adminId;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.CustomerUnblocked);
    }

    public async Task<ApiResponse<string>> ActivateCustomerAsync(int customerId, int adminId)
    {
        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.CustomerDoesNotExist);

        if (customer.Status != UserStatus.Inactive)
            return ApiResponse<string>.Fail(CustomerManagementMessages.NotInactive);

        customer.Status = UserStatus.Active;
        customer.ModifiedAt = DateTime.UtcNow;
        customer.ModifiedBy = adminId;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.CustomerActivated);
    }
    public async Task<ApiResponse<PaymentPagedResult<CustomerBookingListDto>>>
     GetCustomerBookingsAsync(int customerId, CustomerBookingFilterDto filter)
    {
        filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var query =
            from b in _context.Bookings
            join s in _context.Services on b.ServiceId equals s.Id
            join sc in _context.SubCategories on s.SubCategoryId equals sc.Id
            join c in _context.Categories on sc.CategoryId equals c.Id
            join st in _context.ServiceTypes on c.ServiceTypeId equals st.Id
            join p in _context.ServicePartners on b.PartnerId equals p.Id into pg
            from p in pg.DefaultIfEmpty()
            join a in _context.Addresses on b.AddressId equals a.Id
            where b.CustomerId == customerId && b.BookingStatus != BookingStatus.Failed
            select new { Booking = b, Service = s, Partner = p, Address = a, ServiceType = st };


        var amountRange = await query
        .GroupBy(_ => 1)
        .Select(g => new
        {
            Min = g.Min(x => (decimal?)x.Booking.TotalAmount) ?? 0,
            Max = g.Max(x => (decimal?)x.Booking.TotalAmount) ?? 0
        })
        .FirstOrDefaultAsync();

        if (filter.ServiceTypeId.HasValue)
            query = query.Where(x => x.ServiceType.Id == filter.ServiceTypeId.Value);

        if (filter.Date.HasValue)
            query = query.Where(x => x.Booking.SlotDate == filter.Date.Value);

        if (!string.IsNullOrWhiteSpace(filter.Time))
        {
            var t = TimeOnly.Parse(filter.Time);
            query = query.Where(x => x.Booking.SlotStartTime == t);
        }
        if (filter.MinAmount.HasValue)
            query = query.Where(x => x.Booking.TotalAmount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(x => x.Booking.TotalAmount <= filter.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(filter.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(filter.PaymentMethod, true, out var pm))
            query = query.Where(x => x.Booking.PaymentMethod == pm);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<BookingStatus>(filter.Status, true, out var bs))
            query = query.Where(x => x.Booking.BookingStatus == bs);

        bool isDesc = filter.SortOrder?.ToLower() == "desc";
        query = filter.SortBy?.ToLower() switch
        {
            "serviceid" or "id" => isDesc
        ? query.OrderByDescending(x => x.Service.Id)
        : query.OrderBy(x => x.Service.Id),
            "servicename" => isDesc
                ? query.OrderByDescending(x => x.Service.Name)
                : query.OrderBy(x => x.Service.Name),
            "servicetype" => isDesc
                ? query.OrderByDescending(x => x.ServiceType.Name)
                : query.OrderBy(x => x.ServiceType.Name),
            "date" or "datetime" => isDesc
                ? query.OrderByDescending(x => x.Booking.SlotDate)
                : query.OrderBy(x => x.Booking.SlotDate),
            "amount" => isDesc ? query.OrderByDescending(x => x.Booking.TotalAmount) : query.OrderBy(x => x.Booking.TotalAmount),
            _ => query.OrderByDescending(x => x.Booking.Id)
        };

        var totalCount = await query.CountAsync();

        var data = await query
            .AsNoTracking()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new CustomerBookingListDto
            {
                BookingId = x.Booking.Id,
                ServiceId = x.Service.Id,
                ServiceName = x.Service.Name,
                ServiceType = x.ServiceType.Name,
                PartnerId = x.Partner != null ? x.Partner.Id : null,
                IsPartnerDeleted = x.Partner != null && x.Partner.IsDeleted,
                AssignedExpert = x.Partner != null ? x.Partner.FullName : "-",
                PartnerPhone = x.Partner != null ? x.Partner.MobileNumber : null,
                PartnerImage = x.Partner != null ? x.Partner.ProfileImage : null,
                Address = x.Address.HouseFlatNo + ", " + x.Address.Landmark + ", " + x.Address.DisplayName,
                DateTime = x.Booking.SlotDate.ToString("dd MMM yyyy") + " " +
                                 x.Booking.SlotStartTime.ToString("hh:mm tt"),
                Amount = x.Booking.TotalAmount,
                PaymentMethod = x.Booking.PaymentMethod.ToString(),
                Status = x.Booking.BookingStatus.ToString()
            })
            .ToListAsync();

        return ApiResponse<PaymentPagedResult<CustomerBookingListDto>>.SuccessResponse(
            "Bookings fetched successfully",
            new PaymentPagedResult<CustomerBookingListDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                MinAmount = amountRange?.Min ?? 0,
                MaxAmount = amountRange?.Max ?? 0
            });
    }

    public async Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypesAsync()
    {
        var items = await _context.ServiceTypes
            .AsNoTracking()
            .Where(st => !st.IsDeleted)
            .OrderBy(st => st.Name)
            .Select(st => new DropdownOptionDto { Label = st.Name, Value = st.Id.ToString() })
            .AsNoTracking()
            .ToListAsync();

        return ApiResponse<List<DropdownOptionDto>>.SuccessResponse("Service types fetched.", items);
    }

    public async Task<ApiResponse<List<AvailablePartnerDto>>> GetAvailablePartnersAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return ApiResponse<List<AvailablePartnerDto>>.Fail(CustomerManagementMessages.BookingNotFound);

        if (booking.BookingStatus == BookingStatus.Completed ||
            booking.BookingStatus == BookingStatus.Cancelled ||
            booking.BookingStatus == BookingStatus.InProgress)
            return ApiResponse<List<AvailablePartnerDto>>.Fail(CustomerManagementMessages.CannotChangeExpert);

        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == booking.ServiceId);

        if (service == null)
            return ApiResponse<List<AvailablePartnerDto>>.Fail(CustomerManagementMessages.ServiceNotFound);

        var customerEmail = await _context.Customers
            .Where(c => c.Id == booking.CustomerId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(customerEmail))
            return ApiResponse<List<AvailablePartnerDto>>.Fail("Customer email not found.");

        var normalizedCustomerEmail = customerEmail.Trim().ToLower();

        // Get partners who offer this service
        var offeredPartnerIds = await _context.PartnerServicesOffered
            .AsNoTracking()
            .Where(x => x.SubCategoryId == service.SubCategoryId && x.IsActive && !x.IsDeleted)
            .Select(x => x.PartnerId)
            .Distinct()
            .ToListAsync();

        var bookingDate = booking.SlotDate;


        var partners = await _context.ServicePartners
            .AsNoTracking()
            .Where(p =>
                offeredPartnerIds.Contains(p.Id) &&
                !p.IsDeleted &&
                p.Email.ToLower() != normalizedCustomerEmail &&

                (p.Status == PartnerStatus.Active || p.Status == PartnerStatus.Onleave) &&

                !_context.PartnerLeaves.Any(l =>
                    l.PartnerId == p.Id &&
                    l.Status == LeaveStatus.Approved &&
                    bookingDate >= l.FromDate &&
                    bookingDate <= l.ToDate
                )
            )
            .ToListAsync();

        var bufferStart = booking.SlotStartTime.AddMinutes(-15);

        var busyPartnerIds = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.Id != bookingId &&
                b.SlotDate == booking.SlotDate &&
                b.PartnerId != null &&
                partners.Select(p => p.Id).Contains(b.PartnerId!.Value) &&
                b.BookingStatus != BookingStatus.Cancelled &&
                b.BookingStatus != BookingStatus.Completed &&
                b.SlotStartTime < booking.SlotEndTime &&
                b.SlotEndTime > bufferStart)
            .Select(b => b.PartnerId!.Value)
            .Distinct()
            .ToListAsync();

        var availablePartners = partners
            .Where(p => !busyPartnerIds.Contains(p.Id))
            .Select(p => new AvailablePartnerDto
            {
                Id = p.Id,
                FullName = p.FullName,
                ProfileImage = p.ProfileImage,
                MobileNumber = p.MobileNumber,
                IsCurrentlyAssigned = p.Id == booking.PartnerId
            })
            .ToList();

        return ApiResponse<List<AvailablePartnerDto>>.SuccessResponse(
            CustomerManagementMessages.AvailablePartnersFetched,
            availablePartners);
    }

    public async Task<ApiResponse<string>> ChangeExpertAsync(int bookingId, int newPartnerId, int adminId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.BookingNotFound);

        if (booking.BookingStatus == BookingStatus.Completed ||
            booking.BookingStatus == BookingStatus.Cancelled ||
            booking.BookingStatus == BookingStatus.InProgress)
            return ApiResponse<string>.Fail(CustomerManagementMessages.CannotChangeExpert);

        var newPartner = await _context.ServicePartners
            .FirstOrDefaultAsync(p => p.Id == newPartnerId && !p.IsDeleted);

        if (newPartner == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.PartnerNotFound);

        var isOnLeave = await _context.PartnerLeaves.AnyAsync(l =>
            l.PartnerId == newPartnerId &&
            l.Status == LeaveStatus.Approved &&
            booking.SlotDate >= l.FromDate &&
            booking.SlotDate <= l.ToDate
        );

        if (isOnLeave)
            return ApiResponse<string>.Fail("Partner is on leave for selected date.");

        booking.PartnerId = newPartnerId;
        booking.ModifiedAt = DateTime.UtcNow;
        booking.ModifiedBy = adminId;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.ExpertChanged);
    }
    public async Task<ApiResponse<string>> CompleteBookingAsync(int bookingId, int adminId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.BookingNotFound);

        if (booking.BookingStatus == BookingStatus.Completed)
            return ApiResponse<string>.Fail(CustomerManagementMessages.BookingAlreadyCompleted);

        if (booking.BookingStatus == BookingStatus.Cancelled)
            return ApiResponse<string>.Fail(CustomerManagementMessages.CannotCompleteBooking);

        booking.BookingStatus = BookingStatus.Completed;
        booking.ModifiedAt = DateTime.UtcNow;
        booking.ModifiedBy = adminId;

        if (booking.PaymentMethod == PaymentMethod.Cash)
        {
            booking.PaymentStatus = PaymentStatus.Paid;
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
            if (payment != null)
            {
                payment.PaymentStatus = PaymentStatus.Paid;
                payment.ModifiedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.BookingCompleted);
    }

    public async Task<ApiResponse<string>> CancelBookingAsync(int bookingId, string? reason, int adminId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.BookingNotFound);

        if (booking.BookingStatus == BookingStatus.Cancelled)
            return ApiResponse<string>.Fail(CustomerManagementMessages.BookingAlreadyCancelled);

        if (booking.BookingStatus == BookingStatus.Completed)
            return ApiResponse<string>.Fail(CustomerManagementMessages.CannotCancelBooking);

        booking.BookingStatus = BookingStatus.Cancelled;
        booking.CancellationReason = reason;
        booking.PartnerId = null;
        booking.ModifiedAt = DateTime.UtcNow;
        booking.ModifiedBy = adminId;

        await _context.SaveChangesAsync();

        if (booking.PaymentMethod == PaymentMethod.DebitCard &&
            booking.PaymentStatus == PaymentStatus.Paid)
        {
            await _paymentService.RefundBookingAsync(bookingId);
        }

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.BookingCancelled);
    }
    public async Task<ApiResponse<string>> DeleteBookingAsync(int bookingId, int adminId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return ApiResponse<string>.Fail(CustomerManagementMessages.BookingNotFound);

        if (booking.BookingStatus == BookingStatus.Completed)
            return ApiResponse<string>.Fail(CustomerManagementMessages.BookingCompletedNoDelete);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId && !p.IsDeleted);

        if (payment != null)
        {
            payment.IsDeleted = true;
            payment.ModifiedAt = DateTime.UtcNow;
        }

        booking.IsDeleted = true;
        booking.ModifiedBy = adminId;
        booking.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerManagementMessages.BookingDeleted);
    }
}