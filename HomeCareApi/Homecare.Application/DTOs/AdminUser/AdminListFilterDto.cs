using Homecare.Domain.Enums;

namespace Homecare.Application.DTOs.AdminUser;

public class AdminListFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? UserName { get; set; }
    public AdminRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "id";
    public string SortOrder { get; set; } = "desc";
}
