using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Homecare.Application.Services.Shared;

public static class PdfExportHelper
{
    public static byte[] Generate(
    string title, 
    List<string> headers, 
    List<List<string>> rows, 
    List<float>? weights = null) 
{
    var tableHeaders = new List<string> { "#" };
    tableHeaders.AddRange(headers);

    var indexedRows = rows.Select((row, index) =>
    {
        var newRow = new List<string> { (index + 1).ToString() };
        newRow.AddRange(row);
        return newRow;
    }).ToList();

    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Size(PageSizes.A4.Landscape());

            page.Header().PaddingBottom(10).Text(title).FontSize(16).SemiBold();

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(35); 

                    for (int i = 1; i < tableHeaders.Count; i++)
                    {
                        float weight = (weights != null && weights.Count >= i) ? weights[i - 1] : 1;
                        c.RelativeColumn(weight);
                    }
                });

                table.Header(header =>
                    {
                        foreach (var h in tableHeaders)
                        {
                            header.Cell()
                                .Background("#5F5E5A")
                                .Padding(5)
                                .Text(h)
                                .FontColor("#ffffff")
                                .FontSize(10)
                                .SemiBold();
                        }
                    });
                
                foreach (var (row, i) in indexedRows.Select((r, i) => (r, i)))
                {
                    var bg = i % 2 == 0 ? "#ffffff" : "#F1EFE8";

                    foreach (var cell in row)
                    {
                        table.Cell().Element(cellContainer =>
                        {
                            cellContainer
                                .ShowEntire() 
                                .Background(bg)
                                .Padding(5)
                                .Text(cell ?? string.Empty)
                                .FontSize(9);
                        });
                    }
                }
            });

            page.Footer()
                    .PaddingTop(11)
                    .Row(row =>
                    {
                        row.RelativeItem().Text($"Generated on {DateTime.Now:dd MMM yyyy}")
                            .FontSize(9)
                            .FontColor("#333333");

                        row.ConstantItem(120).AlignRight().Text(text =>
                        {
                            text.Span("Page ").FontSize(9).FontColor("#333333");;
                            text.CurrentPageNumber().FontSize(9).FontColor("#333333");;
                            text.Span(" of ").FontSize(9).FontColor("#333333");;
                            text.TotalPages().FontSize(9).FontColor("#333333");;
                        });
                    });
            }); 
        }); 

        var generatedPdf = document.GeneratePdf();

        if (generatedPdf.Length < 4 || (char)generatedPdf[0] != '%')
            throw new InvalidOperationException("QuestPDF generated invalid output.");

        return generatedPdf;
}
}