namespace Mindspace.Services;

public interface IExportService
{
    Task<string> ExportEntriesToPdfAsync(int userId, DateTime from, DateTime to);
}