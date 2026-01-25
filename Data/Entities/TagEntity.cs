using SQLite;

namespace Mindspace.Data.Entities;

[Table("Tags")]
public class TagEntity
{
    [PrimaryKey, AutoIncrement]
    public int TagID { get; set; }

    [Indexed(Unique = true)]
    public string TagName { get; set; } = string.Empty;
}