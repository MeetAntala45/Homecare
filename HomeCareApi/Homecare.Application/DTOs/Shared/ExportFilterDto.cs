namespace Homecare.Application.DTOs.Shared;

public class ExportFilterDto
{
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Dictionary<string, string?> Extra { get; set; } = new();
}