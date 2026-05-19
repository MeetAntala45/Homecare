using Microsoft.AspNetCore.Http;

namespace Homecare.Application.DTOs;

public class UpsertServiceRequestDto
{
    public int? Id { get; set; }
    public int SubCategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal CommissionPct { get; set; }
    public int DurationMin { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Inclusions { get; set; } = new();
    public List<string> Exclusions { get; set; } = new();
    public List<IFormFile>? Images { get; set; }
    public List<string>? ExistingImagePaths { get; set; }
}