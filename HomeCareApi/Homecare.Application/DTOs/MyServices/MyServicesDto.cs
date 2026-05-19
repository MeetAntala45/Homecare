namespace Homecare.Application.DTOs.MyServices;
public class PartnerServiceTypeHierarchyResponseDto
{
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public List<PartnerCategoryResponseDto> Categories { get; set; } = new();
}
public class PartnerCategoryResponseDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<PartnerSubCategoryResponseDto> SubCategories { get; set; } = new();
}

public class PartnerSubCategoryResponseDto
{
    public int SubCategoryId { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public List<PartnerServiceResponseDto> Services { get; set; } = new();
}

public class PartnerServiceResponseDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
}

public class AddPartnerSkillAndServiceRequestDto
{
    public List<int> CategoryIds { get; set; } = new();
    public List<int> SubCategoryIds { get; set; } = new();
}

public class RemovePartnerSubCategoryRequestDto
{
    public int SubCategoryId { get; set; }
}