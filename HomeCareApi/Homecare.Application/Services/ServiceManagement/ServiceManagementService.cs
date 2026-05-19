using Homecare.Application.Constants;
using Homecare.Application.DTOs;
using Homecare.Application.Interfaces;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services;

public class ServiceManagementService : IServiceManagementService
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinary;

    public ServiceManagementService(AppDbContext context, ICloudinaryService cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }

    public async Task<ApiResponse<List<ServiceResponseDto>>> GetAllServices()
    {
        var result = await _context.Services
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Select(s => new ServiceResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                SubCategoryId = s.SubCategoryId,
                SubCategoryName = s.SubCategory.Name,
                Price = s.Price,
                CommissionPct = s.CommissionPct,
                DurationMin = s.DurationMin,
                IsAvailable = s.IsAvailable,

                ImagePaths = s.ServiceImages
                    .Where(i => !i.IsDeleted)
                    .Select(i => i.ImagePath)
                    .ToList(),
            })
            .ToListAsync();

        return ApiResponse<List<ServiceResponseDto>>
            .SuccessResponse("Services fetched successfully.", result);
    }

    public async Task<ApiResponse<List<ServiceResponseDto>>> GetServicesByCategoryAsync(
    int categoryId,
    ServiceFilterDto filter
)
    {
        var query = _context.Services
            .Include(s => s.SubCategory)
            .Include(s => s.ServiceImages)
            .Include(s => s.ServiceChecklists)
            .Where(s =>
                !s.IsDeleted &&
                s.SubCategory.CategoryId == categoryId &&
                (!filter.SubCategoryId.HasValue || s.SubCategoryId == filter.SubCategoryId) &&
                (!filter.PriceMin.HasValue || s.Price >= filter.PriceMin) &&
                (!filter.PriceMax.HasValue || s.Price <= filter.PriceMax) &&
                (!filter.IsAvailable.HasValue || s.IsAvailable == filter.IsAvailable) &&
                (!filter.CommissionPct.HasValue || s.CommissionPct >= filter.CommissionPct)
            )
            .AsNoTracking()
            .AsSplitQuery()
            .AsQueryable();


        bool isDesc = string.IsNullOrWhiteSpace(filter.SortOrder) || filter.SortOrder.ToLower() == "desc";

        query = filter.SortBy?.ToLower() switch
        {
            "id" => isDesc ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
            "name" => isDesc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "subcategoryname" => isDesc ? query.OrderByDescending(s => s.SubCategory.Name) : query.OrderBy(s => s.SubCategory.Name),
            "price" => isDesc ? query.OrderByDescending(s => s.Price) : query.OrderBy(s => s.Price),
            "commissionpct" => isDesc ? query.OrderByDescending(s => s.CommissionPct) : query.OrderBy(s => s.CommissionPct),
            _ => query.OrderByDescending(s => s.Id)
        };

        var services = await query.ToListAsync();
        var result = services.Select(MapToDto).ToList();

        return ApiResponse<List<ServiceResponseDto>>.SuccessResponse(
            "Services fetched successfully.",
            result
        );
    }
    public async Task<ApiResponse<ServiceResponseDto>> GetServiceByIdAsync(int id)
    {
        var dto = await (
            from s in _context.Services

            join sc in _context.SubCategories
                on s.SubCategoryId equals sc.Id

            join c in _context.Categories
                on sc.CategoryId equals c.Id

            join st in _context.ServiceTypes
                on c.ServiceTypeId equals st.Id

            where s.Id == id && !s.IsDeleted

            select new ServiceResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                SubCategoryId = s.SubCategoryId,
                CategoryName = c.Name,
                ServiceTypeName = st.Name,
                SubCategoryName = sc.Name,
                Description = s.Description,
                Price = s.Price,
                CommissionPct = s.CommissionPct,
                DurationMin = s.DurationMin,
                IsAvailable = s.IsAvailable,

                ImagePaths = s.ServiceImages
                    .Where(i => !i.IsDeleted)
                    .Select(i => i.ImagePath)
                    .ToList(),

                Inclusions = s.ServiceChecklists
                    .Where(x => !x.IsDeleted && x.Type == ChecklistType.Inclusion)
                    .Select(x => x.ItemText)
                    .ToList(),

                Exclusions = s.ServiceChecklists
                    .Where(x => !x.IsDeleted && x.Type == ChecklistType.Exclusion)
                    .Select(x => x.ItemText)
                    .ToList()
            }
        ).AsNoTracking().FirstOrDefaultAsync();

        if (dto == null)
            return ApiResponse<ServiceResponseDto>.Fail($"Service with id {id} not found.");

        return ApiResponse<ServiceResponseDto>.SuccessResponse(
            "Service fetched successfully.",
            dto
        );
    }

    public async Task<ApiResponse<List<ServiceResponseDto>>> GetServiceBySubCategoryIdAsync(int subCategoryId)
    {
        var services = await _context.Services
            .Include(s => s.SubCategory)
            .Include(s => s.ServiceImages)
            .Include(s => s.ServiceChecklists)
            .Where(s => !s.IsDeleted && s.SubCategoryId == subCategoryId)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        var result = services.Select(MapToDto).ToList();

        return ApiResponse<List<ServiceResponseDto>>.SuccessResponse(
            "Services fetched successfully.",
            result
        );
    }

    public async Task<ApiResponse<List<ServiceResponseDto>>> GetServiceByServiceTypeIdAsync(int serviceTypeId, string search)
    {
        var query = _context.Services
        .Join(_context.SubCategories,
            s => s.SubCategoryId,
            sc => sc.Id,
            (s, sc) => new { s, sc })
        .Join(_context.Categories,
            temp => temp.sc.CategoryId,
            c => c.Id,
            (temp, c) => new { temp.s, c })
        .Where(x => !x.s.IsDeleted && x.c.ServiceTypeId == serviceTypeId)
        .Select(x => x.s)
        .Include(s => s.ServiceImages)
        .Include(s => s.ServiceChecklists)
        .AsNoTracking()
        .AsSplitQuery()
        .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }
        var services = await query.ToListAsync();

        var result = services.Select(MapToDto).ToList();

        return ApiResponse<List<ServiceResponseDto>>.SuccessResponse(
            "Services fetched successfully.",
            result
        );
    }

    public async Task<ApiResponse<ServiceResponseDto>> UpsertServiceAsync(
    UpsertServiceRequestDto dto,
    int userId
)
    {
        var now = DateTime.UtcNow;


        var subCategoryExists = await _context.SubCategories
            .AnyAsync(s => s.Id == dto.SubCategoryId);

        if (!subCategoryExists)
            return ApiResponse<ServiceResponseDto>.Fail("SubCategory not found.");

        Service service;

        if (dto.Id == null)
        {
            var isDuplicate = await _context.Services.AnyAsync(s =>
                s.SubCategoryId == dto.SubCategoryId &&
                s.Name.ToLower() == dto.Name.ToLower() &&
                !s.IsDeleted
            );

            if (isDuplicate)
                return ApiResponse<ServiceResponseDto>.Fail(
                    "Service already exists in this subcategory."
                );

            service = new Service
            {
                SubCategoryId = dto.SubCategoryId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CommissionPct = dto.CommissionPct,
                DurationMin = dto.DurationMin,
                IsAvailable = dto.IsAvailable,
                CreatedBy = userId,
                CreatedOn = now,
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }
        else
        {
            service = await _context.Services
                .Include(s => s.ServiceImages)
                .Include(s => s.ServiceChecklists)
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.IsDeleted);

            if (service == null)
                return ApiResponse<ServiceResponseDto>.Fail("Service not found.");

            var isDuplicate = await _context.Services.AnyAsync(s =>
                s.SubCategoryId == dto.SubCategoryId &&
                s.Name == dto.Name &&
                s.Id != dto.Id &&
                !s.IsDeleted
            );

            if (isDuplicate)
                return ApiResponse<ServiceResponseDto>.Fail(
                    "Service already exists in this subcategory."
                );

            service.SubCategoryId = dto.SubCategoryId;
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.CommissionPct = dto.CommissionPct;
            service.DurationMin = dto.DurationMin;
            service.IsAvailable = dto.IsAvailable;
            service.ModifiedBy = userId;
            service.ModifiedOn = now;
        }

        if (dto.Id != null)
        {
            var activeImages = service.ServiceImages.Where(i => !i.IsDeleted).ToList();
            foreach (var image in activeImages)
            {
                if (dto.ExistingImagePaths == null || !dto.ExistingImagePaths.Contains(image.ImagePath))
                {
                    // Delete from Cloudinary instead of local disk
                    var publicId = _cloudinary.ExtractPublicId(image.ImagePath);
                    if (publicId != null)
                        await _cloudinary.DeleteAsync(publicId);

                    image.IsDeleted = true;
                    image.ModifiedBy = userId;
                    image.ModifiedOn = now;
                }
            }
        }

        if (dto.Images != null && dto.Images.Count > 0)
        {
            foreach (var image in dto.Images)
            {
                // Upload to Cloudinary instead of wwwroot
                var url = await _cloudinary.UploadImageAsync(image, "service-images");
                service.ServiceImages.Add(new ServiceImage
                {
                    ServiceId = service.Id,
                    ImagePath = url,
                    CreatedOn = now,
                    CreatedBy = userId,
                });
            }
        }

        if (dto.Id != null)
        {
            var existing = service.ServiceChecklists.Where(c => !c.IsDeleted).ToList();

            foreach (var item in existing)
            {
                var sourceList = item.Type == ChecklistType.Inclusion
                    ? dto.Inclusions
                    : dto.Exclusions;

                if (!sourceList.Contains(item.ItemText))
                {
                    item.IsDeleted = true;
                    item.ModifiedBy = userId;
                    item.ModifiedOn = now;
                }
            }
        }

        var existingTexts = service.ServiceChecklists
            .Where(c => !c.IsDeleted)
            .Select(c => c.ItemText)
            .ToHashSet();

        foreach (var item in dto.Inclusions)
        {
            if (!existingTexts.Contains(item))
            {
                service.ServiceChecklists.Add(new ServiceChecklist
                {
                    ServiceId = service.Id,
                    Type = ChecklistType.Inclusion,
                    ItemText = item,
                    CreatedOn = now,
                    CreatedBy = userId,
                });
            }
        }

        foreach (var item in dto.Exclusions)
        {
            if (!existingTexts.Contains(item))
            {
                service.ServiceChecklists.Add(new ServiceChecklist
                {
                    ServiceId = service.Id,
                    Type = ChecklistType.Exclusion,
                    ItemText = item,
                    CreatedOn = now,
                    CreatedBy = userId,
                });
            }
        }

        await _context.SaveChangesAsync();

        var responseDto = await _context.Services
    .Where(x => x.Id == service.Id)
    .Select(x => new ServiceResponseDto
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        SubCategoryName = x.SubCategory.Name,
        Price = x.Price,
        CommissionPct = x.CommissionPct,
        DurationMin = x.DurationMin,
        IsAvailable = x.IsAvailable,
        ImagePaths = x.ServiceImages
            .Where(i => !i.IsDeleted)
            .Select(i => i.ImagePath)
            .ToList()
    })
    .FirstAsync();

        return ApiResponse<ServiceResponseDto>.SuccessResponse(
            dto.Id == null ? "Service created successfully." : "Service updated successfully.",
            responseDto
        );
    }

    public async Task<ApiResponse<bool>> UpdateAvailabilityAsync(
        int id,
        bool isAvailable,
        int modifiedBy
    )
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return ApiResponse<bool>.Fail($"Service {id} not found.");

        service.IsAvailable = isAvailable;
        service.ModifiedBy = modifiedBy;
        service.ModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Availability updated successfully.", true);
    }

    public async Task<ApiResponse<bool>> DeleteServiceAsync(int id, int userId)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return ApiResponse<bool>.Fail($"Service {id} not found.");

        service.IsDeleted = true;
        service.ModifiedBy = userId;
        service.ModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Service deleted successfully.", true);
    }

    private static ServiceResponseDto MapToDto(Service s) =>
        new()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            SubCategoryId = s.SubCategoryId,
            SubCategoryName = s.SubCategory?.Name ?? "",
            Price = s.Price,
            CommissionPct = s.CommissionPct,
            DurationMin = s.DurationMin,
            IsAvailable = s.IsAvailable,
            ImagePaths = s.ServiceImages
                .Where(i => !i.IsDeleted)
                .Select(i => i.ImagePath)
                .ToList(),
            Inclusions = s.ServiceChecklists
                .Where(c => !c.IsDeleted && c.Type == ChecklistType.Inclusion)
                .Select(c => c.ItemText)
                .ToList(),
            Exclusions = s.ServiceChecklists
                .Where(c => !c.IsDeleted && c.Type == ChecklistType.Exclusion)
                .Select(c => c.ItemText)
                .ToList(),
        };
}