using System.Globalization;
using CsvHelper.Excel;

namespace Infrastructure.Services;

public static class ExcelSerializer
{
    private static readonly CultureInfo Culture = new("ka-GE");

    /// <summary>
    /// Writes data to the Excel file.
    /// </summary>
    /// <example>
    /// <code lang="c#">
    /// var data = Reports
    ///     .Select(report => new
    ///     {
    ///         id = report.Id,
    ///         name = report.Name,
    ///     })
    ///     .ToList();
    /// // how to save
    /// await using var stream = new MemoryStream();
    /// await ExcelSerializer.WriteToStreamAsync("sheet_1", data, stream, ct);
    /// var fileName = $"export-name-{DateTime.UtcNow:u}.xlsx";
    /// await FileDownloader.DownloadFileAsync(fileName, stream, CancellationToken);
    /// </code>
    /// </example>
    public static async Task WriteToStreamAsync<T>(string sheetName, IEnumerable<T> data, Stream stream, CancellationToken ct = default) where T : class
    {
        await using var excelWriter = new ExcelWriter(stream, sheetName, Culture);
        await excelWriter.WriteRecordsAsync(data, ct);
    }
}