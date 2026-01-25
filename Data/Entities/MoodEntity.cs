using SQLite;

namespace Mindspace.Data.Entities;

[Table("Moods")]
public class MoodEntity
{
    [PrimaryKey, AutoIncrement]
    public int MoodId { get; set; }

    [Indexed(Unique = true)]
    public string MoodName { get; set; } = string.Empty;

    public string Emoji { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}