using SQLite;
using Mindspace.Data.Entities;
using Mindspace.Services;
using Mindspace.Models;

namespace Mindspace.Data;
public static class AppDatabase
{
    private static SQLiteAsyncConnection? _database;
    private const string DatabaseName = "Mindspace.db";

    public static async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database != null)
            return _database;

        // Get local app data directory
        var databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            DatabaseName
        );

        _database = new SQLiteAsyncConnection(
            databasePath,
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache
        );

        await CreateTablesAsync(_database);
        await SeedMoodsAsync(_database);

        return _database;
    }

    private static async Task CreateTablesAsync(SQLiteAsyncConnection db)
    {
        await db.CreateTableAsync<UserEntity>();
        try
        {
            await db.ExecuteAsync("ALTER TABLE Users ADD COLUMN PinHash TEXT NULL");
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine("PinHash column already exists: " + ex.Message);
#endif
        }

        await db.CreateTableAsync<CategoryEntity>();
        await db.CreateTableAsync<MoodEntity>();
        await db.CreateTableAsync<TagEntity>();
        await db.CreateTableAsync<JournalEntryEntity>();
        await db.CreateTableAsync<EntryMoodEntity>();
        await db.CreateTableAsync<EntryTagEntity>();
    }

    private static async Task SeedMoodsAsync(SQLiteAsyncConnection db)
    {
        var defaultMoods = MoodService.GetAllMoods();
        
        foreach (var mood in defaultMoods)
        {
            // Check if mood already exists
            var existing = await db.Table<MoodEntity>()
                .Where(m => m.MoodName.ToLower() == mood.Name.ToLower())
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                var moodEntity = new MoodEntity
                {
                    MoodName = mood.Name,
                    Emoji = mood.Emoji,
                    Category = mood.Category
                };
                await db.InsertAsync(moodEntity);
            }
        }
    }
}