using Homecare.Application.Constants;
using Homecare.Application.Interfaces.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homecare.API.Controllers.Shared
{
    [ApiController]
    [Route("api/export")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IEnumerable<IDataExporter> _exporters;

        public ExportController(IEnumerable<IDataExporter> exporters)
        {
            _exporters = exporters;
        }

        [HttpGet("{type}/csv")]
        public async Task<IActionResult> ExportCsv(string type, [FromQuery] Dictionary<string, string> queryParams)
        {
            var exporter = GetExporter(type);
            if (exporter == null)
                return BadRequest(new ApiResponse<string> { Success = false, Message = $"Export type '{type}' not supported." });

            var bytes = await exporter.ExportCsvAsync(queryParams);
            return File(bytes, "text/csv", $"{type}_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("{type}/pdf")]
        public async Task<IActionResult> ExportPdf(string type, [FromQuery] Dictionary<string, string> queryParams)
        {
            var exporter = GetExporter(type);
            if (exporter == null)
                return BadRequest(new ApiResponse<string> { Success = false, Message = $"Export type '{type}' not supported." });

            var bytes = await exporter.ExportPdfAsync(queryParams);
            return File(bytes, "application/pdf", $"{type}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private IDataExporter? GetExporter(string type)
            => _exporters.FirstOrDefault(e => e.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }
}