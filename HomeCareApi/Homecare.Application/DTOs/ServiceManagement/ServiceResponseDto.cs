namespace Homecare.Application.DTOs;

public class ServiceResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string CategoryName {get; set;} =string.Empty;
        public string ServiceTypeName {get; set;}=string.Empty;
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal CommissionPct { get; set; }
        public int DurationMin { get; set; }
        public bool IsAvailable { get; set; }
        public List<string> ImagePaths { get; set; } = new();
        public List<string> Inclusions { get; set; } = new();
        public List<string> Exclusions { get; set; } = new();
    }