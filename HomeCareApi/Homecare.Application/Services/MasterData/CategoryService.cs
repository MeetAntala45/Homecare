using System.Text.RegularExpressions;
using Homecare.Application.Constants.MasterData;
using Homecare.Application.DTOs.MasterData;
using Homecare.Application.Interfaces;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.MasterData;

public class CategoryService : ICategory
{
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IList<CategoryDto>> GetAllCategory(int serviceTypeId)
    {
        var data = await _context.Categories
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.ServiceTypeId == serviceTypeId)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                ServiceTypeId = x.ServiceTypeId
            })
            .ToListAsync();

        return data;
    }

    public async Task AddCategory(CategoryDto dto)
    {
         var normalizedName = Regex.Replace(dto.Name.Trim(), @"\s+", " ");
         
        bool exists = await _context.Categories
            .Where(x => !x.IsDeleted && x.ServiceTypeId == dto.ServiceTypeId)
            .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            throw new InvalidOperationException(CategoryConstant.AlreadyExistsException);

        var entity = new Category
        {
            Name = dto.Name,
            ServiceTypeId = dto.ServiceTypeId
        };

        entity.SetCreated(_currentUser.UserId);

        await _context.Categories.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategory(int id)
    {
        var query = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (query == null)
            throw new KeyNotFoundException(CategoryConstant.NotFoundException);

        bool hasSubCategories = await _context.SubCategories
            .AnyAsync(x => x.CategoryId == id && !x.IsDeleted);

        if (hasSubCategories)
            throw new InvalidOperationException(CategoryConstant.NotEmptyException);

        query.SoftDelete(true);
        query.SetModified(_currentUser.UserId);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategory(int id, CategoryDto dto)
    {
        var query = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (query == null)
            throw new KeyNotFoundException(CategoryConstant.NotFoundException);

        bool exists = await _context.Categories
            .Where(x => x.Id != id && !x.IsDeleted && x.ServiceTypeId == query.ServiceTypeId)
            .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

        if (exists)
            throw new InvalidOperationException(CategoryConstant.AlreadyExistsException);

        query.Name = dto.Name;
        query.SetModified(_currentUser.UserId);

        await _context.SaveChangesAsync();
    }
}