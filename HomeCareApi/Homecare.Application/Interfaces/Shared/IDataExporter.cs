namespace Homecare.Application.Interfaces.Shared;

public interface IDataExporter
{
    string Type { get; }
    Task<byte[]> ExportCsvAsync(Dictionary<string, string> queryParams);
    Task<byte[]> ExportPdfAsync(Dictionary<string, string> queryParams);
}