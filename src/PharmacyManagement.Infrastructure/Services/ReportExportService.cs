using ClosedXML.Excel;
using PharmacyManagement.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ReportExportService : IReportExportService
{
    static ReportExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string GetContentType(ReportExportFormat format)
        => format == ReportExportFormat.Pdf
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string GetFileExtension(ReportExportFormat format)
        => format == ReportExportFormat.Pdf ? ".pdf" : ".xlsx";

    public byte[] ExportExcel(ReportTableDto report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(SanitizeSheetName(report.Title));

        sheet.Cell(1, 1).Value = report.Title;
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Cell(2, 1).Value = string.IsNullOrWhiteSpace(report.Subtitle)
            ? $"Generated {report.GeneratedAtUtc:u}"
            : $"{report.Subtitle} | Generated {report.GeneratedAtUtc:u}";

        for (var columnIndex = 0; columnIndex < report.Columns.Count; columnIndex++)
        {
            var cell = sheet.Cell(4, columnIndex + 1);
            cell.Value = report.Columns[columnIndex];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        for (var rowIndex = 0; rowIndex < report.Rows.Count; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < report.Columns.Count; columnIndex++)
            {
                var value = rowIndex < report.Rows.Count && columnIndex < report.Rows[rowIndex].Count
                    ? report.Rows[rowIndex][columnIndex]
                    : null;
                var cell = sheet.Cell(rowIndex + 5, columnIndex + 1);
                switch (value)
                {
                    case null:
                        cell.Value = string.Empty;
                        break;
                    case decimal dec:
                        cell.Value = dec;
                        break;
                    case double number:
                        cell.Value = number;
                        break;
                    case int number:
                        cell.Value = number;
                        break;
                    case long number:
                        cell.Value = number;
                        break;
                    case DateTime date:
                        cell.Value = date;
                        break;
                    case bool boolean:
                        cell.Value = boolean;
                        break;
                    default:
                        cell.Value = value.ToString() ?? string.Empty;
                        break;
                }
            }
        }

        var summaryRow = report.Rows.Count + 6;
        foreach (var item in report.Summary)
        {
            sheet.Cell(summaryRow, 1).Value = item.Label;
            sheet.Cell(summaryRow, 1).Style.Font.Bold = true;
            sheet.Cell(summaryRow, 2).Value = item.Value;
            summaryRow++;
        }

        if (report.Columns.Count > 0)
        {
            sheet.Range(4, 1, Math.Max(4, report.Rows.Count + 4), report.Columns.Count).SetAutoFilter();
        }

        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportPdf(ReportTableDto report)
    {
        return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(style => style.FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Text(report.Title).FontSize(18).Bold();
                        column.Item().Text(string.IsNullOrWhiteSpace(report.Subtitle)
                            ? $"Generated {report.GeneratedAtUtc:u}"
                            : $"{report.Subtitle} | Generated {report.GeneratedAtUtc:u}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(definition =>
                            {
                                foreach (var _ in report.Columns)
                                {
                                    definition.RelativeColumn();
                                }
                            });

                            table.Header(header =>
                            {
                                foreach (var columnName in report.Columns)
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(columnName).Bold();
                                }
                            });

                            foreach (var row in report.Rows)
                            {
                                foreach (var value in row)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4).Text(FormatValue(value));
                                }
                            }
                        });

                        if (report.Summary.Count > 0)
                        {
                            column.Item().PaddingTop(12).Table(table =>
                            {
                                table.ColumnsDefinition(definition =>
                                {
                                    definition.RelativeColumn(2);
                                    definition.RelativeColumn(3);
                                });

                                foreach (var item in report.Summary)
                                {
                                    table.Cell().Padding(3).Text(item.Label).Bold();
                                    table.Cell().Padding(3).Text(item.Value);
                                }
                            });
                        }
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.CurrentPageNumber();
                    });
                });
            })
            .GeneratePdf();
    }

    private static string FormatValue(object? value)
        => value switch
        {
            null => string.Empty,
            DateTime date => date.ToString("yyyy-MM-dd HH:mm"),
            decimal dec => dec.ToString("N2"),
            double number => number.ToString("N2"),
            _ => value.ToString() ?? string.Empty
        };

    private static string SanitizeSheetName(string title)
    {
        var invalid = Path.GetInvalidFileNameChars().Concat(new[] { '/', '\\', '*', '?', ':', '[', ']' }).Distinct().ToArray();
        var safe = invalid.Aggregate(title, (current, character) => current.Replace(character, ' '));
        if (string.IsNullOrWhiteSpace(safe))
        {
            safe = "Report";
        }

        return safe.Length <= 31 ? safe : safe[..31];
    }
}
