using System.Text.RegularExpressions;
using Homecare.Application.Constants.MasterData;
using Homecare.Application.DTOs.MasterData;
using Homecare.Application.Interfaces;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.MasterData;

public class SubCategoryService : ISubCategory
{
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public SubCategoryService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IList<SubCategoryDto>> GetAllSubCategory(int categoryId)
    {
        return await _context.SubCategories
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.CategoryId == categoryId)
            .Select(x => new SubCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId
            })
            .ToListAsync();
    }

    public async Task AddSubCategory(SubCategoryDto dto)
    {
         var normalizedName = Regex.Replace(dto.Name.Trim(), @"\s+", " ");

        if (dto.CategoryId == 0)
            throw new InvalidOperationException(SubCategoryConstant.InvalidCategoryResponse);

        bool exists = await _context.SubCategories
            .Where(x => !x.IsDeleted && x.CategoryId == dto.CategoryId)
            .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            throw new InvalidOperationException(SubCategoryConstant.AlreadyExistsException);

        var entity = new SubCategory
        {
            Name = dto.Name,
            CategoryId = dto.CategoryId
        };

        entity.SetCreated(_currentUser.UserId);

        await _context.SubCategories.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

   public async Task DeleteSubCategory(int id)
{
    var query = await _context.SubCategories
        .Include(x => x.Services.Where(s => !s.IsDeleted))
        .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

    if (query == null)
        throw new KeyNotFoundException(SubCategoryConstant.NotFoundException);
        
    if (query.Services.Any())
    {
        throw new InvalidOperationException(SubCategoryConstant.SubCategoryHaveServices);
    }

    query.SoftDelete(true);
    query.SetModified(_currentUser.UserId);

    await _context.SaveChangesAsync();
}

    public async Task UpdateSubCategory(int id, SubCategoryDto dto)
    {
        var query = await _context.SubCategories
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (query == null)
            throw new KeyNotFoundException(SubCategoryConstant.NotFoundException);

        bool exists = await _context.SubCategories
            .Where(x => x.Id != id && !x.IsDeleted && x.CategoryId == query.CategoryId)
            .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

        if (exists)
            throw new InvalidOperationException(SubCategoryConstant.AlreadyExistsException);

        query.Name = dto.Name;
        query.SetModified(_currentUser.UserId);

        await _context.SaveChangesAsync();
    }
}