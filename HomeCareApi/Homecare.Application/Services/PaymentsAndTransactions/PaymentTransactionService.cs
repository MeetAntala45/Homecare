using Homecare.Application.Common.Models;
using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.Constants.PaymentsAndTransactions;
using Homecare.Application.DTOs.PaymentsAndTransactions;
using Homecare.Application.Interfaces.PaymentsAndTransactions;
using Homecare.Data;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.PaymentsAndTransactions;

public class PaymentTransactionService : IPaymentTransactionService
{
    private readonly AppDbContext _context;

    public PaymentTransactionService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<ApiResponse<PaymentPagedResult<PaymentListDto>>> GetPaymentsAsync(PaymentListFilterDto filter)
    {
        filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var query = _context.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Service)
            .Where(p =>
                p.PaymentStatus == PaymentStatus.Paid &&
                !p.Booking.IsDeleted &&
                !p.IsDeleted
            )
            .AsSplitQuery()
            .AsQueryable();

        var amountRange = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Min = g.Min(p => (decimal?)p.Amount) ?? 0,
                Max = g.Max(p => (decimal?)p.Amount) ?? 0
            })
            .FirstOrDefaultAsync();

        if (filter.MinAmount.HasValue)
            query = query.Where(p => p.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

        if (!string.IsNullOrEmpty(filter.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(filter.PaymentMethod, true, out var method))
        {
            query = query.Where(p => p.PaymentMethod == method);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            var normalisedName = filter.UserName
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                normalisedName.All(term =>
                    x.Booking.Customer.Name
                        .ToLower()
                        .Contains(term)
                )
            );
        }

        bool isDesc = filter.SortOrder?.ToLower() == "desc";

        query = !string.IsNullOrWhiteSpace(filter.SortBy)
            ? filter.SortBy.ToLower() switch
            {
                "id" => isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "user" => isDesc ? query.OrderByDescending(p => p.Booking.Customer.Name) : query.OrderBy(p => p.Booking.Customer.Name),
                "bookingId" => isDesc ? query.OrderByDescending(p => p.BookingId) : query.OrderBy(p => p.BookingId),
                "service" => isDesc ? query.OrderByDescending(p => p.Booking.Service.Name) : query.OrderBy(p => p.Booking.Service.Name),
                "amount" => isDesc ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
                _ => query.OrderByDescending(p => p.Id)
            }
            : query.OrderByDescending(p => p.Id);

        var totalCount = await query.CountAsync();
        

        var data = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new PaymentListDto
            {
                Id = p.Id,
                UserName = p.Booking.Customer.Name.ToTitleCase(),
                TransactionId = p.TransactionId,
                BookingId = p.BookingId.ToString(),
                MobileNumber = p.Booking.Customer.MobileNumber ?? "-",
                ServiceName = p.Booking.Service.Name,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                CreatedOn = p.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PaymentPagedResult<PaymentListDto>>.SuccessResponse(
            PaymentTransactionMessages.PaymentsFetched,
            new PaymentPagedResult<PaymentListDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                MinAmount = amountRange?.Min ?? 0,
                MaxAmount = amountRange?.Max ?? 0
            }
        );
    }

    public async Task<List<PaymentListDto>> GetAllForExportAsync(PaymentListFilterDto filter, bool paginate = false)
    {
        var query = _context.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Service)
            .Where(p =>
                p.PaymentStatus == PaymentStatus.Paid &&
                !p.Booking.IsDeleted &&
                !p.IsDeleted
            )
            .AsSplitQuery()
            .AsQueryable();

        if (filter.MinAmount.HasValue)
            query = query.Where(p => p.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

        if (!string.IsNullOrEmpty(filter.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(filter.PaymentMethod, true, out var method))
            query = query.Where(p => p.PaymentMethod == method);

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            var terms = filter.UserName.Trim().ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                terms.All(term => x.Booking.Customer.Name.ToLower().Contains(term))
            );
        }
        
        bool isDesc = filter.SortOrder?.ToLower() == "desc";

        query = !string.IsNullOrWhiteSpace(filter.SortBy)
            ? filter.SortBy.ToLower() switch
            {
                "id" => isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "user" => isDesc ? query.OrderByDescending(p => p.Booking.Customer.Name) : query.OrderBy(p => p.Booking.Customer.Name),
                "bookingId" => isDesc ? query.OrderByDescending(p => p.BookingId) : query.OrderBy(p => p.BookingId),
                "service" => isDesc ? query.OrderByDescending(p => p.Booking.Service.Name) : query.OrderBy(p => p.Booking.Service.Name),
                "amount" => isDesc ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
                _ => query.OrderByDescending(p => p.Id)
            }
            : query.OrderByDescending(p => p.Id);
        
        if (paginate && filter.PageNumber > 0 && filter.PageSize > 0)
        {
            query = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }

        return await query
            .Select(p => new PaymentListDto
            {
                Id = p.Id,
                UserName = p.Booking.Customer.Name.ToTitleCase(),
                TransactionId = p.TransactionId,
                BookingId = p.BookingId.ToString(),
                MobileNumber = p.Booking.Customer.MobileNumber ?? "-",
                ServiceName = p.Booking.Service.Name,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                CreatedOn = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<UserPaymentDetailDto>> GetUserPaymentDetailAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Service)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
        {
            return ApiResponse<UserPaymentDetailDto>.Fail(PaymentTransactionMessages.PaymentNotFound);
        }

        var result = new UserPaymentDetailDto
        {
            UserId = payment.Booking.CustomerId,
            UserName = payment.Booking.Customer?.Name ?? "N/A",
            MobileNumber = payment.Booking.Customer?.MobileNumber,
            TransactionId = payment.TransactionId,
            ServiceName = payment.Booking?.Service?.Name ?? "Service",
            ServiceId = payment.Booking?.ServiceId ?? 0,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(),
            TransactionDateTime = payment.CreatedAt
        };

        return ApiResponse<UserPaymentDetailDto>.SuccessResponse(PaymentTransactionMessages.PaymentDetailsFetched, result);
    }

    public async Task<ApiResponse<PaymentPagedResult<UserPaymentListDto>>> GetUserPaymentsAsync(UserPaymentFilterDto filter)
    {
        filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var query = _context.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Service)
            .Where(p =>
                p.Booking.CustomerId == filter.UserId &&
                p.Id != filter.CurrentPaymentId &&
                p.PaymentStatus == PaymentStatus.Paid &&
                !p.Booking.IsDeleted &&
                !p.IsDeleted
            )
            .AsSplitQuery()
            .AsQueryable();

        var amountRange = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Min = g.Min(p => (decimal?)p.Amount) ?? 0,
                Max = g.Max(p => (decimal?)p.Amount) ?? 0
            })
            .FirstOrDefaultAsync();

        if (filter.MinAmount.HasValue)
            query = query.Where(p => p.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

        if (!string.IsNullOrEmpty(filter.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(filter.PaymentMethod, true, out var method))
        {
            query = query.Where(p => p.PaymentMethod == method);
        }

        bool isDesc = filter.SortOrder?.ToLower() == "desc";

        query = !string.IsNullOrWhiteSpace(filter.SortBy)
            ? filter.SortBy.ToLower() switch
            {
                "id" => isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "bookingid" => isDesc ? query.OrderByDescending(p => p.BookingId) : query.OrderBy(p => p.BookingId),
                "service" => isDesc ? query.OrderByDescending(p => p.Booking.Service.Name) : query.OrderBy(p => p.Booking.Service.Name),
                "amount" => isDesc ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
                _ => query.OrderByDescending(p => p.Id)
            }
            : query.OrderByDescending(p => p.Id);

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new UserPaymentListDto
            {
                Id = p.Id,
                TransactionId = p.TransactionId,
                BookingId = p.BookingId.ToString(),
                ServiceName = p.Booking.Service.Name,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString()
            })
            .ToListAsync();

        return ApiResponse<PaymentPagedResult<UserPaymentListDto>>.SuccessResponse(
            PaymentTransactionMessages.UserPaymentsFetched,
            new PaymentPagedResult<UserPaymentListDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                MinAmount = amountRange?.Min ?? 0,
                MaxAmount = amountRange?.Max ?? 0
            }
        );
    }

    public async Task<List<UserPaymentExportDto>> GetAllUserPaymentForExportAsync(UserPaymentFilterDto filter, bool paginate = false)
    {
         var query = _context.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Service)
            .Where(p =>
                p.Booking.CustomerId == filter.UserId &&
                p.Id != filter.CurrentPaymentId &&
                p.PaymentStatus == PaymentStatus.Paid &&
                !p.Booking.IsDeleted &&
                !p.IsDeleted
            )
            .AsSplitQuery()
            .AsQueryable();

        if (filter.MinAmount.HasValue)
            query = query.Where(p => p.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

        if (!string.IsNullOrEmpty(filter.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(filter.PaymentMethod, true, out var method))
        {
            query = query.Where(p => p.PaymentMethod == method);
        }

        bool isDesc = filter.SortOrder?.ToLower() == "desc";

        query = !string.IsNullOrWhiteSpace(filter.SortBy)
            ? filter.SortBy.ToLower() switch
            {
                "id" => isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "bookingid" => isDesc ? query.OrderByDescending(p => p.BookingId) : query.OrderBy(p => p.BookingId),
                "service" => isDesc ? query.OrderByDescending(p => p.Booking.Service.Name) : query.OrderBy(p => p.Booking.Service.Name),
                "amount" => isDesc ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
                _ => query.OrderByDescending(p => p.Id)
            }
            : query.OrderByDescending(p => p.Id);

        if (paginate && filter.PageNumber > 0 && filter.PageSize > 0)
        {
            query = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }

        return await query
            .Select(p => new UserPaymentExportDto
            {
                Id = p.Id,
                TransactionId = p.TransactionId,
                BookingId = p.BookingId.ToString(),
                ServiceName = p.Booking.Service.Name,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                TransactionDateTime = p.CreatedAt
            })
            .ToListAsync();
    }
}
