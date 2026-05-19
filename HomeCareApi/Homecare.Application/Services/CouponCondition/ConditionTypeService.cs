using System;
using Homecare.Application.Constants;
using Homecare.Application.Constants.CouponCondition;
using Homecare.Application.Constants.Offers;
using Homecare.Application.DTOs.CouponCondition;
using Homecare.Application.Interfaces.CouponCondition;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Offers;

public class ConditionTypeService : IConditionTypeService
{
    private readonly AppDbContext _context;

    public ConditionTypeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ConditionTypeResponseDto>>> GetAllActiveAsync()
    {
        var data = await _context.CouponConditionTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .Select(x => MapToDto(x))
            .ToListAsync();

        return ApiResponse<List<ConditionTypeResponseDto>>
            .SuccessResponse(ConditionTypeMessages.FetchSuccess, data);
    }

    public async Task<ApiResponse<List<ConditionTypeResponseDto>>> GetAllAsync()
    {
        var data = await _context.CouponConditionTypes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToDto(x))
            .ToListAsync();

        return ApiResponse<List<ConditionTypeResponseDto>>
            .SuccessResponse(ConditionTypeMessages.FetchSuccess, data);
    }

    public async Task<ApiResponse<string>> CreateAsync(
       CreateConditionTypeDto dto, int adminId)
    {

        var labelExists = await _context.CouponConditionTypes
        .AnyAsync(x => x.Label.ToLower() == dto.Label.Trim().ToLower());

        if (labelExists)
            return ApiResponse<string>.Fail(
                ConditionTypeMessages.LabelAlreadyExists);

        var entity = new CouponConditionType
        {
            Label = dto.Label.Trim(),
            ContextKey = dto.ContextKey.Trim().ToLower(),
            InputType = dto.InputType.Trim().ToLower(),
            DefaultOperator = dto.DefaultOperator.Trim().ToLower(),
            DefaultFailBehaviour = dto.DefaultFailBehaviour.Trim().ToLower(),
            IsActive = true,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.CouponConditionTypes.AddAsync(entity);
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(ConditionTypeMessages.CreateSuccess);
    }

    private static ConditionTypeResponseDto MapToDto(CouponConditionType x)
        => new()
        {
            Id = x.Id,
            Label = x.Label,
            ContextKey = x.ContextKey,
            InputType = x.InputType,
            DefaultOperator = x.DefaultOperator,
            DefaultFailBehaviour = x.DefaultFailBehaviour,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt
        };

    public ApiResponse<List<string>> GetContextKeys()
    {
        var keys = ContextKeys.All.ToList();

        return ApiResponse<List<string>>.SuccessResponse(
            OfferMessages.ContextKeysFetched,
            keys
        );
    }

    public ApiResponse<List<string>> GetAllowedOperators(string inputType)
    {
        if (!InputTypeOperators.Allowed.TryGetValue(inputType.ToLower(), out var ops))
        {
            return ApiResponse<List<string>>.Fail(OfferMessages.UnknownInputType);
        }

        return ApiResponse<List<string>>.SuccessResponse(
            OfferMessages.OperatorsFetched,
            ops.ToList()
        );
    }

    public ApiResponse<List<string>> GetAllowedInputTypes(string contextKey)
    {
        if (!ContextKeys.AllowedInputTypes.TryGetValue(contextKey.ToLower(), out var types))
        {
            return ApiResponse<List<string>>.Fail(OfferMessages.UnknownContextKey);
        }

        return ApiResponse<List<string>>.SuccessResponse(
            OfferMessages.InputTypesFetched,
            types.ToList()
        );
    }

    public async Task<ApiResponse<List<ServiceTypeGroupDto>>> GetServiceTypeHierarchyAsync()
    {
        var subCategories = await _context.SubCategories
        .AsNoTracking()
        .Where(sc => !sc.IsDeleted)
        .Select(sc => new { sc.Id, sc.Name, sc.CategoryId })
        .ToListAsync();

        var categoryIds = subCategories.Select(sc => sc.CategoryId).Distinct().ToList();

        var categories = await _context.Categories
        .AsNoTracking()
        .Where(c => categoryIds.Contains(c.Id) && !c.IsDeleted)
        .Select(c => new { c.Name, c.Id, c.ServiceTypeId })
        .ToListAsync();

        var serviceTypeIds = categories.Select(c => c.ServiceTypeId).Distinct().ToList();

        var serviceTypes = await _context.ServiceTypes
            .AsNoTracking()
            .Where(st => serviceTypeIds.Contains(st.Id) && !st.IsDeleted)
            .Select(st => new { st.Id, st.Name })
            .OrderBy(st => st.Name)
            .ToListAsync();

        var result = serviceTypes.Select(st => new ServiceTypeGroupDto
        {
            ServiceTypeId = st.Id,
            ServiceTypeName = st.Name,
            Categories = categories
            .Where(c => c.ServiceTypeId == st.Id)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryGroupDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                Subcategories = subCategories
                    .Where(sc => sc.CategoryId == c.Id)
                    .OrderBy(sc => sc.Name)
                    .Select(sc => new SubcategoryOptionDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                    })
                    .ToList()
            })
            .Where(c => c.Subcategories.Any())
            .ToList()
        })
        .Where(st => st.Categories.Any())
        .ToList();

        return ApiResponse<List<ServiceTypeGroupDto>>
            .SuccessResponse("Hierarchy fetched.", result);
    }
}
