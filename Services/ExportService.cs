using Mindspace.Models;
using PdfSharpCore.Pdf;
using System.Text;

namespace Mindspace.Services;

public class ExportService : IExportService
{
    private readonly IJournalService _journalService;

    public ExportService(IJournalService journalService)
    {
        _journalService = journalService;
    }

    public async Task<string> ExportEntriesToPdfAsync(int userId, DateTime from, DateTime to)
    {
        var entries = await _journalService.GetAllEntriesAsync(userId);

        var filtered = entries
            .Where(e => e.Date.Date >= from.Date && e.Date.Date <= to.Date)
            .OrderBy(e => e.Date)
            .ToList();

        var html = BuildHtml(filtered, from, to);

        var doc = new PdfDocument();
        // PdfGenerator.AddPdfPages(doc, html, PdfSharpCore.PageSize.A4);

        var fileName = $"MindSpace_Journal_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf";
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using var stream = File.Create(path);
        doc.Save(stream);

        return path;
    }

    private static string BuildHtml(List<JournalEntry> entries, DateTime from, DateTime to)
    {
        var sb = new StringBuilder();
        sb.Append($@"
<html>
<head>
<meta charset='utf-8' />
<style>
body {{ font-family: Arial, sans-serif; font-size: 12px; }}
h1 {{ font-size: 18px; }}
.entry {{ margin-bottom: 18px; padding-bottom: 10px; border-bottom: 1px solid #ccc; }}
.title {{ font-size: 14px; font-weight: bold; }}
.meta {{ color: #555; margin: 4px 0; }}
.tags {{ color: #666; }}
</style>
</head>
<body>
<h1>MindSpace Journal Export ({from:MMM dd, yyyy} - {to:MMM dd, yyyy})</h1>
");

        if (entries.Count == 0)
        {
            sb.Append("<p>No entries found in this date range.</p>");
        }
        else
        {
            foreach (var e in entries)
            {
                sb.Append("<div class='entry'>");
                sb.Append($"<div class='title'>{e.Date:MMMM dd, yyyy} — {Escape(e.Title)}</div>");
                sb.Append($"<div class='meta'>Category: {Escape(e.Category)} | Primary Mood: {Escape(e.PrimaryMood)}</div>");

                if (e.Tags.Any())
                    sb.Append($"<div class='tags'>Tags: {Escape(string.Join(", ", e.Tags))}</div>");

                // If your Content is HTML from Quill, keep it as-is. If plain, wrap it.
                sb.Append($"<div class='content'>{e.Content}</div>");
                sb.Append("</div>");
            }
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static string Escape(string? s)
        => System.Net.WebUtility.HtmlEncode(s ?? "");
}
