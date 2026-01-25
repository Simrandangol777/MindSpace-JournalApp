using SQLite;
namespace Mindspace.Data.Entities;

[Table("Categories")]
public class CategoryEntity
{
    [PrimaryKey, AutoIncrement]
    public int CategoryID { get; set; }

    [Indexed(Unique = true)]
    public string CategoryName { get; set; } = string.Empty;
}