using SQLite;

namespace Mindspace.Data.Entities;

[Table("JournalEntries")]
public class JournalEntryEntity
{
    [PrimaryKey, AutoIncrement]
    public int EntryID { get; set; }
    [Indexed]
    public int UserID { get; set; }
    [Indexed]
    public int? CategoryID { get; set; }

    public DateTime EntryDate { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

}