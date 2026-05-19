using System.Text;

namespace Homecare.Application.Services.Shared;

public static class CsvExportHelper
{
    public static byte[] Generate(List<string> headers, List<List<string>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers));

        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
        => value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}