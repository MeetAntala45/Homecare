using System.Text.RegularExpressions;
using Homecare.Application.Constants.MasterData;
using Homecare.Application.DTOs.MasterData;
using Homecare.Application.Interfaces;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.MasterData;

public class ServiceTypeService : IServiceType
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _context;

    public ServiceTypeService(AppDbContext context, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _context = context;
        _currentUser = currentUser;
        _env = env;
    }

    public async Task<IList<ServiceTypeDto>> GetAllServiceTypes()
    {
        return await _context.ServiceTypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive)
            .Select(x => new ServiceTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                ImagePath = x.ImagePath
            })
            .ToListAsync();
    }

    public async Task AddServiceType(ServiceTypeDto dto)
    {
        var normalizedName = Regex.Replace(dto.Name.Trim(), @"\s+", " ");
        
        bool exists = await _context.ServiceTypes.AnyAsync(x =>
            !x.IsDeleted &&
            x.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            throw new InvalidOperationException(ServiceTypeConstant.AlreadyExistsException);

        var entity = new ServiceType
        {
            Name = dto.Name,
            ImagePath = dto.ImagePath
        };

        entity.SetCreated(_currentUser.UserId);

        await _context.ServiceTypes.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteServiceType(int id)
    {
        var query = await _context.ServiceTypes
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (query == null)
            throw new KeyNotFoundException(ServiceTypeConstant.NotFoundException);

        bool hasCategories = await _context.Categories
            .AnyAsync(x => x.ServiceTypeId == id && !x.IsDeleted);

        if (hasCategories)
            throw new InvalidOperationException(ServiceTypeConstant.NotNullServiceType);

        if (!string.IsNullOrWhiteSpace(query.ImagePath))
        {
            DeleteImage(query.ImagePath);
        }

        query.SoftDelete(true);
        query.SetModified(_currentUser.UserId);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateServiceType(int id, ServiceTypeDto dto)
    {
        var normalizedName = Regex.Replace(dto.Name.Trim(), @"\s+", " ");
        
       
        var query = await _context.ServiceTypes
            .FirstOrDefaultAsync(x => x.Id == id);

        if (query == null)
            throw new KeyNotFoundException(ServiceTypeConstant.NotFoundException);

        bool exists = await _context.ServiceTypes.AnyAsync(x =>
            x.Id != id &&
            !x.IsDeleted &&
            x.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            throw new InvalidOperationException(ServiceTypeConstant.AlreadyExistsException);

        query.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(query.ImagePath))
        {
            DeleteImage(query.ImagePath);
        }

        query.ImagePath = dto.ImagePath;

        query.SetModified(_currentUser.UserId);

        await _context.SaveChangesAsync();
    }

    private void DeleteImage(string imagePath)
    {

        var fullPath = Path.Combine(
            _env.WebRootPath,
            imagePath.TrimStart('/')
        );

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}