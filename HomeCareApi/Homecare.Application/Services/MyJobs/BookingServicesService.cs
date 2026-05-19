using Homecare.Application.Interfaces.MyJobs;
using Homecare.Data;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.MyJobs
{
    public class BookingServicesService : IBookingServicesService
    {
        private readonly AppDbContext _context;

        public BookingServicesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetDistinctServiceNamesAsync(int partnerId)
        {
            return await _context.Bookings
                .Where(b => b.PartnerId == partnerId)
                .Join(
                    _context.Services,
                    b => b.ServiceId,
                    s => s.Id,
                    (b, s) => s.Name
                )
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }
    }
}