using SQLite;

namespace Mindspace.Data.Entities;

[Table("EntryMoods")]
public class EntryMoodEntity
{
    [PrimaryKey, AutoIncrement]
    public int EntryMoodId { get; set; }

    /// <summary>
    /// Foreign key to JournalEntryEntity.EntryID
    /// </summary>
    [Indexed]
    public int EntryID { get; set; }

    /// <summary>
    /// Foreign key to MoodEntity.MoodId
    /// </summary>
    [Indexed]
    public int MoodID { get; set; }

    /// <summary>
    /// "Primary" or "Secondary"
    /// </summary>
    public string MoodType { get; set; } = string.Empty;
}