using System;

namespace Homecare.Application.DTOs.CouponCondition;

public class ServiceTypeGroupDto
{
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = null!;
    public List<CategoryGroupDto> Categories { get; set; } = new();
}

public class CategoryGroupDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public List<SubcategoryOptionDto> Subcategories { get; set; } = new();
}

public class SubcategoryOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
