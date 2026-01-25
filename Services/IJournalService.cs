using Mindspace.Models;

namespace Mindspace.Services;
public interface IJournalService
{
    Task<List<JournalEntry>> GetAllEntriesAsync(int userId);
    Task<JournalEntry?> GetEntryByIdAsync(int id);
    Task<JournalEntry?> GetEntryByDateAsync(int userId, DateTime date);
    Task<(bool Success, string Message, JournalEntry? Entry)> CreateEntryAsync(JournalEntry entry);
    Task<(bool Success, string Message)> UpdateEntryAsync(JournalEntry entry);
    Task<(bool Success, string Message)> DeleteEntryAsync(int id);
    Task<List<string>> GetAllTagsAsync();
}