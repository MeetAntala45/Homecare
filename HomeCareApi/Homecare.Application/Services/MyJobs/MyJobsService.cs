using System.Linq.Expressions;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.MyJobs;
using Homecare.Application.Interfaces.MyJobs;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services
{
    public class MyJobsService : IMyJobsService
    {
        private readonly AppDbContext _context;

        public MyJobsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentPagedResult<MyJobsDto>> GetBookingsByPartnerIdAsync(MyJobRequestDto req, int userId)
        {
            var pageNumber = req.PageNumber <= 0 ? 1 : req.PageNumber;
            var pageSize = req.PageSize <= 0 ? 10 : req.PageSize;
            var sortBy = req.SortBy?.Trim().ToLower();
            var sortOrder = req.SortOrder?.Trim().ToLower();

            var query =
                from b in _context.Bookings
                join s in _context.Services on b.ServiceId equals s.Id
                join c in _context.Customers on b.CustomerId equals c.Id
                join a in _context.Addresses on b.AddressId equals a.Id
                where b.PartnerId == userId
                select new
                {
                    b,
                    s,
                    c,
                    a,
                    NetAmount = b.TotalAmount - (b.TotalAmount * (s.CommissionPct / 100)) - (b.TotalAmount * 0.10m)
                };

            var serviceNames = await _context.Bookings
                .Where(b => b.PartnerId == userId)
                .Join(_context.Services,
                    b => b.ServiceId,
                    s => s.Id,
                    (b, s) => s.Name)
                .Distinct()
                .ToListAsync();

            if (req.MinAmount.HasValue)
            {
                query = query.Where(x => x.NetAmount >= req.MinAmount.Value);
            }

            if (req.MaxAmount.HasValue)
            {
                query = query.Where(x => x.NetAmount <= req.MaxAmount.Value);
            }
            if (!string.IsNullOrEmpty(req.ServiceName))
            {
                var service = req.ServiceName.ToLower();
                query = query.Where(x => x.s.Name.ToLower().Contains(service));
            }

            if (!string.IsNullOrEmpty(req.CustomerName))
            {
                var customer = req.CustomerName.ToLower();
                query = query.Where(x => x.c.Name.ToLower().Contains(customer));
            }
            if (req.BookingDate.HasValue)
            {
                query = query.Where(x => x.b.SlotDate == req.BookingDate.Value);
            }
            if (!string.IsNullOrEmpty(req.PaymentMethod))
            {
                var method = req.PaymentMethod.ToLower();

                query = query.Where(x =>
                    x.b.PaymentMethod.ToString().ToLower() == method
                );
            }

            if (!string.IsNullOrEmpty(req.Status))
            {
                var status = req.Status.ToLower();

                query = status switch
                {
                    "pending" => query.Where(x => x.b.BookingStatus == BookingStatus.Pending),
                    "completed" => query.Where(x => x.b.BookingStatus == BookingStatus.Completed),
                    _ => query
                };
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = (sortBy, sortOrder) switch
                {
                    ("servicename", "asc") => query.OrderBy(x => x.s.Name),
                    ("servicename", "desc") => query.OrderByDescending(x => x.s.Name),

                    ("customername", "asc") => query.OrderBy(x => x.c.Name),
                    ("customername", "desc") => query.OrderByDescending(x => x.c.Name),

                    ("bookingdate", "asc") => query.OrderBy(x => x.b.SlotDate),
                    ("bookingdate", "desc") => query.OrderByDescending(x => x.b.SlotDate),

                    ("bookingstatus", "asc") => query.OrderBy(x => x.b.BookingStatus),
                    ("bookingstatus", "desc") => query.OrderByDescending(x => x.b.BookingStatus),

                    ("amount", "asc") => query.OrderBy(x => x.NetAmount),
                    ("amount", "desc") => query.OrderByDescending(x => x.NetAmount),

                    _ => query
                };
            }
            else
            {

                if (req.Status?.ToLower() == "pending")
                {
                    query = query
                        .OrderBy(x => x.b.SlotDate)
                        .ThenBy(x => x.b.SlotStartTime);
                }
                else
                {
                    query = query
                        .OrderByDescending(x => x.b.SlotDate)
                        .ThenByDescending(x => x.b.SlotStartTime);
                }
            }


            var totalCount = await query.CountAsync();
            var minAmount = await query.MinAsync(x => (decimal?)x.NetAmount) ?? 0;
            var maxAmount = await query.MaxAsync(x => (decimal?)x.NetAmount) ?? 0;

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MyJobsDto
                {
                    BookingId = x.b.Id,
                    ServiceName = x.s.Name,
                    CustomerName = x.c.Name,
                    Address =
                        (x.a.HouseFlatNo ?? "") +
                        (!string.IsNullOrEmpty(x.a.Landmark) ? ", " + x.a.Landmark : "") +
                        (!string.IsNullOrEmpty(x.a.DisplayName) ? ", " + x.a.DisplayName : ""),
                    BookingDate = x.b.SlotDate,
                    SlotTime = x.b.SlotStartTime,
                    Amount = x.NetAmount,
                    BookingStatus = x.b.BookingStatus.ToString(),
                    PaymentMethod = x.b.PaymentMethod.ToString()
                })
                .ToListAsync();

            return new PaymentPagedResult<MyJobsDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                MinAmount = minAmount,
                MaxAmount = maxAmount
            };
        }

        public async Task<List<MyJobCalendarDto>> GetCalendarJobsAsync(MyJobCalendarRequestDto req, int userId)
        {
            var startDate = new DateOnly(req.Year, req.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            Expression<Func<Booking, int>> statusOrder = b =>
                b.BookingStatus == BookingStatus.InProgress ? 1 :
                b.BookingStatus == BookingStatus.Pending ? 2 :
                b.BookingStatus == BookingStatus.Completed ? 3 : 4;

            var data = await _context.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.PartnerId == userId &&
                    b.SlotDate >= startDate &&
                    b.SlotDate <= endDate &&
                    b.BookingStatus != BookingStatus.Failed &&
                    b.BookingStatus != BookingStatus.Cancelled)
                .OrderBy(b => b.SlotDate)
                .ThenBy(statusOrder)
                .ThenBy(b => b.SlotStartTime)
                .Select(b => new MyJobCalendarDto
                {
                    BookingId = b.Id,
                    ServiceName = b.Service.Name,
                    CustomerName = b.Customer.Name,
                    BookingDate = b.SlotDate,
                    SlotTime = b.SlotStartTime,
                    BookingStatus = b.BookingStatus.ToString()
                }
            ).ToListAsync();

            return data;
        }

        public async Task<int> GetBookingPageAsync(int partnerId, int bookingId,
        int pageSize, string status)
        {
            var isPending = status.ToLower() == "pending";

            var query = _context.Bookings
                .Where(b => b.PartnerId == partnerId && !b.IsDeleted)
                .Where(b => isPending
                    ? b.BookingStatus == BookingStatus.Pending ||
                      b.BookingStatus == BookingStatus.InProgress
                    : b.BookingStatus == BookingStatus.Completed)
                .OrderByDescending(b => b.SlotDate)
                .ThenByDescending(b => b.SlotStartTime)
                .Select(b => b.Id);

            var list = await query.ToListAsync();
            var index = list.IndexOf(bookingId);

            return index < 0 ? 1 : (index / pageSize) + 1;
        }
    }
}