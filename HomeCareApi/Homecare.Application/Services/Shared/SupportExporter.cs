using Homecare.Application.DTOs.Support.cs;
using Homecare.Application.Interfaces.Shared;
using Homecare.Application.Interfaces.Support;

namespace Homecare.Application.Services.Shared;

public class SupportExporter : IDataExporter
{
    private readonly ISupportService _supportService;

    public string Type => "support";

    public SupportExporter(ISupportService supportService)
    {
        _supportService = supportService;
    }

    public async Task<byte[]> ExportCsvAsync(Dictionary<string, string> queryParams)
    {
        var (filter, paginate) = BuildFilter(queryParams);
        var data = await _supportService.GetAllForExportAsync(filter, paginate);

        var headers = new List<string>
            { "Ticket No", "Name", "Email", "Mobile Number", "Description", "Submitted On" };

        var rows = data.Select(s => new List<string>
        {
            s.Id.ToString(),
            s.UserName,
            s.Email,
            s.Mobile ?? "-",
            s.Description,
            s.CreatedAt.ToString("dd MMM yyyy")
        }).ToList();

        return CsvExportHelper.Generate(headers, rows);
    }

    public async Task<byte[]> ExportPdfAsync(Dictionary<string, string> queryParams)
    {
        var (filter, paginate) = BuildFilter(queryParams);
        var data = await _supportService.GetAllForExportAsync(filter, paginate);

        var headers = new List<string>
            { "Ticket No", "Name", "Email", "Mobile", "Description", "Submitted On" };

        var columnWeights = new List<float> 
            { 1.4f, 2.0f, 2.7f, 1.7f, 5.0f, 1.8f };

        var rows = data.Select(s => new List<string>
        {
            s.Id.ToString(),
            s.UserName,
            s.Email,
            s.Mobile ?? "-",
            s.Description ?? "-",
            s.CreatedAt.ToString("dd MMM yyyy")
        }).ToList();

        return PdfExportHelper.Generate("Support Tickets", headers, rows, columnWeights);
    }

    private (SupportFilterRequest filter, bool paginate) BuildFilter(Dictionary<string, string> p)
    {
        var userName = QueryParamParser.GetString(p, "userName");
        var dateString = QueryParamParser.GetString(p, "createdDate");
        var sortBy = QueryParamParser.GetString(p, "sortBy");
        var sortOrder = QueryParamParser.GetString(p, "sortOrder");
        var pageNumber = QueryParamParser.GetInt(p, "pageNumber");
        var pageSize = QueryParamParser.GetInt(p, "pageSize");

        DateTime? createdDate = !string.IsNullOrEmpty(dateString) && DateTime.TryParse(dateString, out var parsedDate) 
                                ? parsedDate 
                                : null;

        var paginate = pageNumber > 0 && pageSize > 0;

        var filter = new SupportFilterRequest
        {
            UserName = userName,
            CreatedDate = createdDate,
            SortBy = sortBy,
            SortOrder = sortOrder,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return (filter, paginate);
    }
}