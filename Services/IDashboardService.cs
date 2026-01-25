using Mindspace.Models;

namespace Mindspace.Services;

public interface IDashboardService
{
    Task<List<JournalEntry>> GetRecentEntriesAsync(int userId, int count);
    Task<DashboardStats> GetStatsAsync(int userId);
}