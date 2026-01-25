using SQLite;

namespace Mindspace.Data.Entities;

[Table("EntryTags")]
public class EntryTagEntity
{
    [PrimaryKey, AutoIncrement]
    public int EntryTagID { get; set; }

    /// <summary>
    /// Foreign key to JournalEntryEntity.EntryID
    /// </summary>
    [Indexed]
    public int EntryID { get; set; }

    /// <summary>
    /// Foreign key to TagEntity.TagID
    /// </summary>
    [Indexed]
    public int TagID { get; set; }
}