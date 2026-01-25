using SQLite;
namespace Mindspace.Data.Entities;

[Table("Users")]
public class UserEntity
{
    [PrimaryKey, AutoIncrement]
    public int UserID { get; set; }

    [Indexed(Unique = true)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? PinHash { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }
}