using Homecare.Application.Constants;
using Homecare.Application.DTOs.MyServices;
using Homecare.Application.Interfaces.MyService;
using Homecare.Data;
using Microsoft.EntityFrameworkCore;
using PartnerSkillEntity = Homecare.Domain.Entities.PartnerSkill;
using PartnerServiceOfferedEntity = Homecare.Domain.Entities.PartnerServiceOffered;

namespace Homecare.Application.Services.MyService;

public class MyServices : IMyService
{
    private readonly AppDbContext _context;

    public MyServices(AppDbContext context)
    {
        _context = context;
    }
    public async Task<ApiResponse<List<PartnerServiceTypeHierarchyResponseDto>>> GetPartnerServiceHierarchyAsync(int partnerId)
    {
        try
        {
            var partner = await _context.ServicePartners
                .Include(p => p.Skills)
                .Include(p => p.ServicesOffered)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == partnerId && !p.IsDeleted);

            if (partner == null)
            {
                return ApiResponse<List<PartnerServiceTypeHierarchyResponseDto>>.Fail(
                    $"Service partner with id {partnerId} not found.");
            }

            var serviceTypes = await _context.ServiceTypes
            .AsNoTracking()
            .Where(st => !st.IsDeleted && st.Id == partner.ServiceTypeId)
            .ToListAsync();

            var categoryIds = partner.Skills
                .Where(x => x.IsDeleted == false)
                .Select(x => x.CategoryId)
                .Distinct()
                .ToList();

            var subCategoryIds = partner.ServicesOffered
                .Select(x => x.SubCategoryId)
                .Distinct()
                .ToList();

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => categoryIds.Contains(c.Id) && !c.IsDeleted)
                .Join(
                    _context.ServiceTypes.AsNoTracking().Where(st => !st.IsDeleted),
                    c => c.ServiceTypeId,
                    st => st.Id,
                    (c, st) => new
                    {
                        c.Id,
                        c.Name,
                        c.ServiceTypeId,
                        ServiceTypeName = st.Name
                    }
                )
                .ToListAsync();

            var subCategories = await _context.SubCategories
                .AsNoTracking()
                .Where(sc => subCategoryIds.Contains(sc.Id) && !sc.IsDeleted)
                .Select(sc => new
                {
                    sc.Id,
                    sc.Name,
                    sc.CategoryId
                })
                .ToListAsync();

            var services = await _context.Services
                .AsNoTracking()
                .Where(s => subCategoryIds.Contains(s.SubCategoryId) && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.SubCategoryId
                })
                .ToListAsync();

            var result = serviceTypes.Select(st => new PartnerServiceTypeHierarchyResponseDto
            {
                ServiceTypeId = st.Id,
                ServiceTypeName = st.Name,

                Categories = categories
                    .Where(c => c.ServiceTypeId == st.Id)
                    .Select(category => new PartnerCategoryResponseDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,

                        SubCategories = subCategories
                            .Where(sc => sc.CategoryId == category.Id)
                            .Select(sc => new PartnerSubCategoryResponseDto
                            {
                                SubCategoryId = sc.Id,
                                SubCategoryName = sc.Name,

                                Services = services
                                    .Where(s => s.SubCategoryId == sc.Id)
                                    .Select(s => new PartnerServiceResponseDto
                                    {
                                        ServiceId = s.Id,
                                        ServiceName = s.Name
                                    })
                                .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

            return ApiResponse<List<PartnerServiceTypeHierarchyResponseDto>>.SuccessResponse(
                "Partner service hierarchy fetched successfully.",
                result);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PartnerServiceTypeHierarchyResponseDto>>.Fail(ex.Message);
        }
    }
    public async Task<ApiResponse<string>> AddSkillAndServiceAsync(int userId, AddPartnerSkillAndServiceRequestDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var partner = await _context.ServicePartners
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (partner == null)
                return ApiResponse<string>.Fail("Service partner not found");

            if (partner.IsDeleted)
                return ApiResponse<string>.Fail("Service partner is inactive");

            var partnerId = partner.Id;

            var validCategoryIds = await _context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id) && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            if (!validCategoryIds.Any())
                return ApiResponse<string>.Fail("Invalid Categories");

            var validSubCategories = await _context.SubCategories
                .Where(sc => request.SubCategoryIds.Contains(sc.Id) && !sc.IsDeleted)
                .ToListAsync();

            if (!validSubCategories.Any())
                return ApiResponse<string>.Fail("Invalid SubCategories");

            foreach (var categoryId in validCategoryIds)
            {
                var skillExists = await _context.PartnerSkills
                    .AnyAsync(x => x.PartnerId == partnerId && x.CategoryId == categoryId);

                if (!skillExists)
                {
                    await _context.PartnerSkills.AddAsync(new PartnerSkillEntity
                    {
                        PartnerId = partnerId,
                        CategoryId = categoryId
                    });
                }
            }

            foreach (var sub in validSubCategories)
            {
                var serviceExists = await _context.PartnerServicesOffered
                    .AnyAsync(x => x.PartnerId == partnerId && x.SubCategoryId == sub.Id);

                if (!serviceExists)
                {
                    await _context.PartnerServicesOffered.AddAsync(new PartnerServiceOfferedEntity
                    {
                        PartnerId = partnerId,
                        SubCategoryId = sub.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<string>.SuccessResponse("Skills & Services added successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<string>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> RemoveSkillServiceAsync(int partnerId, int subCategoryId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var partner = await _context.ServicePartners
                .FirstOrDefaultAsync(x => x.Id == partnerId && !x.IsDeleted);

            if (partner == null)
                return ApiResponse<string>.Fail("Service partner not found");

            var subCategoryEntity = await _context.PartnerServicesOffered
                .FirstOrDefaultAsync(x =>
                    x.PartnerId == partnerId &&
                    x.SubCategoryId == subCategoryId);

            if (subCategoryEntity == null)
                return ApiResponse<string>.Fail("SubCategory not assigned");

            _context.PartnerServicesOffered.Remove(subCategoryEntity);

            await _context.SaveChangesAsync();

            var categoryId = await _context.SubCategories
                .Where(x => x.Id == subCategoryId)
                .Select(x => x.CategoryId)
                .FirstOrDefaultAsync();

            var stillHasSubCategories = await _context.PartnerServicesOffered
                .AnyAsync(x =>
                    x.PartnerId == partnerId &&
                    _context.SubCategories
                        .Where(sc => sc.CategoryId == categoryId)
                        .Select(sc => sc.Id)
                        .Contains(x.SubCategoryId)
                );


            if (!stillHasSubCategories)
            {
                var categoryEntity = await _context.PartnerSkills
                    .FirstOrDefaultAsync(x =>
                        x.PartnerId == partnerId &&
                        x.CategoryId == categoryId &&
                        !x.IsDeleted);

                _context.PartnerSkills.Remove(categoryEntity!);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<string>.SuccessResponse("Removed successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<string>.Fail($"Error: {ex.Message}");
        }
    }
}