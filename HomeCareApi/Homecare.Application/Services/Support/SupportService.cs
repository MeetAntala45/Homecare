using Homecare.Data;
using Homecare.Application.Interfaces.Support;
using Microsoft.EntityFrameworkCore;
using Homecare.Application.DTOs.Support.cs;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.Constants.Support;
using Homecare.Application.Common.Models;

namespace Homecare.Application.Services.Support;

public class SupportService : ISupportService
{
    private readonly AppDbContext _context;

    public SupportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddContactAsync(SupportCreateDto dto)
    {
        bool exists = await _context.Supports.AnyAsync(x =>
            x.Email.ToLower() == dto.Email.ToLower() &&
            x.Description.ToLower() == dto.Description.ToLower());

        if (exists)
            throw new InvalidOperationException(SupportConstant.InvalidOperationException);

        var contact = new Domain.Entities.Support
        {
            FirstName = dto.FirstName.ToTitleCase(),
            LastName = dto.LastName.ToTitleCase(),
            Mobile = dto.Mobile!,
            Email = dto.Email.ToLower(),
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Supports.Add(contact);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<SupportResponseDto>> GetAllContactsAsync(SupportFilterRequest req)
    {
        var query = _context.Supports.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.UserName))
        {
            var terms = req.UserName
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                terms.All(term =>
                    (x.FirstName + " " + x.LastName)
                        .ToLower()
                        .Contains(term)
                )
            );
        }
        if (req.CreatedDate.HasValue)
        {
            var date = req.CreatedDate.Value.Date;

            query = query.Where(x => x.CreatedAt >= date && x.CreatedAt < date.AddDays(1));

        }
        var totalCount = await query.CountAsync();

        query = (req.SortBy?.ToLower(), req.SortOrder?.ToLower()) switch
        {
            ("id", "asc") => query.OrderBy(x => x.Id),
            ("id", "desc") => query.OrderByDescending(x => x.Id),

            ("username", "asc") => query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName),
            ("username", "desc") => query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName),

            ("email", "asc") => query.OrderBy(x => x.Email),
            ("email", "desc") => query.OrderByDescending(x => x.Email),

            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),

            _ => query.OrderByDescending(x => x.Id)
        };

        var data = await query
        .Skip((req.PageNumber - 1) * req.PageSize)
        .Take(req.PageSize)
        .Select(x => new SupportResponseDto
        {
            Id = x.Id,
            UserName = (x.FirstName + " " + x.LastName).Trim().ToTitleCase(),
            Mobile = x.Mobile,
            Email = x.Email.ToLower(),
            Description = x.Description,
            CreatedAt = x.CreatedAt
        })
        .ToListAsync();

        return new PagedResult<SupportResponseDto>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize
        };
    }

    public async Task<List<SupportResponseDto>> GetAllForExportAsync(SupportFilterRequest filter, bool paginate = false)
    {
        var query = _context.Supports.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            var terms = filter.UserName
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                terms.All(term =>
                    (x.FirstName + " " + x.LastName)
                        .ToLower()
                        .Contains(term)
                )
            );
        }

        if (filter.CreatedDate.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAt.Date == filter.CreatedDate.Value.Date 
            );
        }

        query = (filter.SortBy?.ToLower(), filter.SortOrder?.ToLower()) switch
        {
            ("id", "asc") => query.OrderBy(x => x.Id),
            ("id", "desc") => query.OrderByDescending(x => x.Id),

            ("username", "asc") => query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName),
            ("username", "desc") => query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName),

            ("email", "asc") => query.OrderBy(x => x.Email),
            ("email", "desc") => query.OrderByDescending(x => x.Email),

            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),

            _ => query.OrderByDescending(x => x.Id)
        };

        if (paginate && filter.PageNumber > 0 && filter.PageSize > 0)
        {
            query = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }

        return await query.Select(s => new SupportResponseDto
        {
            Id = s.Id,
            UserName = (s.FirstName + " " + s.LastName).ToTitleCase(),
            Email = s.Email,
            Mobile = s.Mobile,
            Description = s.Description,
            CreatedAt = s.CreatedAt
        })
        .ToListAsync();
    }
}