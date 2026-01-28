namespace Mindspace.Services;

public interface IExportService
{
    Task ExportEntriesToPdfAsync(int userId, DateTime from, DateTime to);
}