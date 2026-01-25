using Mindspace.Models;
using Mindspace.Data;
using Mindspace.Data.Entities;
using SQLite;

namespace Mindspace.Services;

public class JournalService : IJournalService
    {
        private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static DateTime ToDateOnly(DateTime dt) => dt.Date;

    private static async Task<SQLiteAsyncConnection> DbAsync()
        => await AppDatabase.GetDatabaseAsync();

    private static async Task<int?> GetOrCreateMoodIdAsync(SQLiteAsyncConnection db, string moodName)
    {
        if (string.IsNullOrWhiteSpace(moodName)) return null;
        
        var name = moodName.Trim();

        var mood = await db.Table<MoodEntity>()
            .Where(m => m.MoodName.ToLower() == name.ToLower())
            .FirstOrDefaultAsync();

        if (mood != null) return mood.MoodId;

        // If mood doesn't exist, try to find it in the predefined list and create it
        var defaultMoods = MoodService.GetAllMoods();
        var matchingMood = defaultMoods.FirstOrDefault(m => 
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (matchingMood != null)
        {
            var moodEntity = new MoodEntity
            {
                MoodName = matchingMood.Name,
                Emoji = matchingMood.Emoji,
                Category = matchingMood.Category
            };
            await db.InsertAsync(moodEntity);
            return moodEntity.MoodId;
        }

        // If not in predefined list, return null (mood doesn't exist and can't be created)
        return null;
    }

    private static async Task<int?> GetOrCreateCategoryIdAsync(SQLiteAsyncConnection db, string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) return null;

        var name = categoryName.Trim();

        var existing = await db.Table<CategoryEntity>()
            .Where(c => c.CategoryName == name)
            .FirstOrDefaultAsync();

        if (existing != null) return existing.CategoryID;

        var entity = new CategoryEntity { CategoryName = name };
        await db.InsertAsync(entity);
        return entity.CategoryID;
    }

    private static async Task<int> GetOrCreateTagIdAsync(SQLiteAsyncConnection db, string tagName)
    {
        var name = tagName.Trim();

        var existing = await db.Table<TagEntity>()
            .Where(t => t.TagName == name)
            .FirstOrDefaultAsync();

        if (existing != null) return existing.TagID;

        var entity = new TagEntity { TagName = name };
        await db.InsertAsync(entity);
        return entity.TagID;
    }

    // ---------- Mapping (Entity -> UI Model) ----------

    private static async Task<JournalEntry> BuildJournalEntryModelAsync(SQLiteAsyncConnection db, JournalEntryEntity e)
    {
        // Category
        string categoryName = "";
        if (e.CategoryID.HasValue)
        {
            var cat = await db.Table<CategoryEntity>()
                .Where(c => c.CategoryID == e.CategoryID.Value)
                .FirstOrDefaultAsync();
            categoryName = cat?.CategoryName ?? "";
        }

        // Moods (primary + secondary)
        var entryMoods = await db.Table<EntryMoodEntity>()
            .Where(em => em.EntryID == e.EntryID)
            .ToListAsync();

        string primaryMood = "";
        var secondaryMoods = new List<string>();

        if (entryMoods.Count > 0)
        {
            var moodIds = entryMoods.Select(x => x.MoodID).Distinct().ToList();
            var moods = await db.Table<MoodEntity>()
                .Where(m => moodIds.Contains(m.MoodId))
                .ToListAsync();

            var moodById = moods.ToDictionary(m => m.MoodId, m => m.MoodName);

            foreach (var em in entryMoods)
            {
                if (!moodById.TryGetValue(em.MoodID, out var moodName)) continue;

                if (string.Equals(em.MoodType, "Primary", StringComparison.OrdinalIgnoreCase))
                    primaryMood = moodName;
                else if (string.Equals(em.MoodType, "Secondary", StringComparison.OrdinalIgnoreCase))
                    secondaryMoods.Add(moodName);
            }
        }

        // Tags
        var entryTags = await db.Table<EntryTagEntity>()
            .Where(et => et.EntryID == e.EntryID)
            .ToListAsync();

        var tags = new List<string>();
        if (entryTags.Count > 0)
        {
            var tagIds = entryTags.Select(x => x.TagID).Distinct().ToList();
            var tagRows = await db.Table<TagEntity>()
                .Where(t => tagIds.Contains(t.TagID))
                .ToListAsync();

            var tagById = tagRows.ToDictionary(t => t.TagID, t => t.TagName);
            foreach (var et in entryTags)
                if (tagById.TryGetValue(et.TagID, out var tname))
                    tags.Add(tname);
        }

        return new JournalEntry
        {
            Id = e.EntryID,
            UserId = e.UserID,
            Date = e.EntryDate,
            Title = e.Title,
            Content = e.Content,
            Category = categoryName,
            PrimaryMood = primaryMood,
            SecondaryMoods = secondaryMoods,
            Tags = tags,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            WordCount = CountWords(e.Content)
        };
    }

    // ---------- Public CRUD ----------

    public async Task<List<JournalEntry>> GetAllEntriesAsync(int userId)
    {
        var db = await DbAsync();

        // Load all entries for the user
        var entities = await db.Table<JournalEntryEntity>()
            .Where(e => e.UserID == userId)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();

        if (entities.Count == 0)
            return new List<JournalEntry>();

        var entryIds = entities.Select(e => e.EntryID).ToList();
        var categoryIds = entities.Where(e => e.CategoryID.HasValue).Select(e => e.CategoryID!.Value).Distinct().ToList();

        // Batch load all related data
        var entryMoods = await db.Table<EntryMoodEntity>()
            .Where(em => entryIds.Contains(em.EntryID))
            .ToListAsync();

        var moodIds = entryMoods.Select(em => em.MoodID).Distinct().ToList();
        var moods = moodIds.Count > 0
            ? await db.Table<MoodEntity>()
                .Where(m => moodIds.Contains(m.MoodId))
                .ToListAsync()
            : new List<MoodEntity>();

        var entryTags = await db.Table<EntryTagEntity>()
            .Where(et => entryIds.Contains(et.EntryID))
            .ToListAsync();

        var tagIds = entryTags.Select(et => et.TagID).Distinct().ToList();
        var tags = tagIds.Count > 0
            ? await db.Table<TagEntity>()
                .Where(t => tagIds.Contains(t.TagID))
                .ToListAsync()
            : new List<TagEntity>();

        var categories = categoryIds.Count > 0
            ? await db.Table<CategoryEntity>()
                .Where(c => categoryIds.Contains(c.CategoryID))
                .ToListAsync()
            : new List<CategoryEntity>();

        // Create lookup dictionaries for efficient joins
        var moodById = moods.ToDictionary(m => m.MoodId, m => m.MoodName);
        var tagById = tags.ToDictionary(t => t.TagID, t => t.TagName);
        var categoryById = categories.ToDictionary(c => c.CategoryID, c => c.CategoryName);

        // Group entry moods by entry ID
        var moodsByEntry = entryMoods
            .GroupBy(em => em.EntryID)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Group entry tags by entry ID
        var tagsByEntry = entryTags
            .GroupBy(et => et.EntryID)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Build result using LINQ joins
        var result = entities.Select(e =>
        {
            // Get category
            string categoryName = "";
            if (e.CategoryID.HasValue && categoryById.TryGetValue(e.CategoryID.Value, out var catName))
            {
                categoryName = catName;
            }

            // Get moods
            string primaryMood = "";
            var secondaryMoods = new List<string>();

            if (moodsByEntry.TryGetValue(e.EntryID, out var entryMoodList))
            {
                foreach (var em in entryMoodList)
                {
                    if (!moodById.TryGetValue(em.MoodID, out var moodName)) continue;

                    if (string.Equals(em.MoodType, "Primary", StringComparison.OrdinalIgnoreCase))
                        primaryMood = moodName;
                    else if (string.Equals(em.MoodType, "Secondary", StringComparison.OrdinalIgnoreCase))
                        secondaryMoods.Add(moodName);
                }
            }

            // Get tags
            var entryTagsList = new List<string>();
            if (tagsByEntry.TryGetValue(e.EntryID, out var entryTagList))
            {
                foreach (var et in entryTagList)
                {
                    if (tagById.TryGetValue(et.TagID, out var tagName))
                        entryTagsList.Add(tagName);
                }
            }

            return new JournalEntry
            {
                Id = e.EntryID,
                UserId = e.UserID,
                Date = e.EntryDate,
                Title = e.Title,
                Content = e.Content,
                Category = categoryName,
                PrimaryMood = primaryMood,
                SecondaryMoods = secondaryMoods,
                Tags = entryTagsList,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                WordCount = CountWords(e.Content)
            };
        }).ToList();

        return result;
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        var db = await DbAsync();

        var e = await db.Table<JournalEntryEntity>()
            .Where(x => x.EntryID == id)
            .FirstOrDefaultAsync();

        if (e == null) return null;
        return await BuildJournalEntryModelAsync(db, e);
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(int userId, DateTime date)
    {
        var db = await DbAsync();
        var day = ToDateOnly(date);

        var e = await db.Table<JournalEntryEntity>()
            .Where(x => x.UserID == userId && x.EntryDate == day)
            .FirstOrDefaultAsync();

        if (e == null) return null;
        return await BuildJournalEntryModelAsync(db, e);
    }

    public async Task<(bool Success, string Message, JournalEntry? Entry)> CreateEntryAsync(JournalEntry entry)
    {
        var db = await DbAsync();

        // Validation (same as your in-memory service, but now real DB)
        if (entry.UserId <= 0) return (false, "Invalid user.", null);
        if (string.IsNullOrWhiteSpace(entry.Title)) return (false, "Title is required.", null);
        if (string.IsNullOrWhiteSpace(entry.PrimaryMood)) return (false, "Primary mood is required.", null);
        if (entry.SecondaryMoods.Count > 2) return (false, "Maximum 2 secondary moods allowed.", null);

        var day = ToDateOnly(entry.Date);

        // Enforce one-entry-per-day-per-user
        var existing = await db.Table<JournalEntryEntity>()
            .Where(e => e.UserID == entry.UserId && e.EntryDate == day)
            .FirstOrDefaultAsync();

        if (existing != null)
            return (false, "An entry already exists for this day.", null);

        var categoryId = await GetOrCreateCategoryIdAsync(db, entry.Category);

        var now = DateTime.Now;
        var entity = new JournalEntryEntity
        {
            UserID = entry.UserId,
            CategoryID = categoryId,
            EntryDate = day,
            Title = entry.Title.Trim(),
            Content = entry.Content ?? "",
            CreatedAt = now,
            UpdatedAt = now
        };

        await db.InsertAsync(entity); 

        var primaryMoodId = await GetOrCreateMoodIdAsync(db, entry.PrimaryMood);
        if (primaryMoodId == null)
            return (false, $"Unknown primary mood: {entry.PrimaryMood}", null);

        await db.InsertAsync(new EntryMoodEntity
        {
            EntryID = entity.EntryID,
            MoodID = primaryMoodId.Value,
            MoodType = "Primary"
        });

        // Insert Secondary moods
        foreach (var sm in entry.SecondaryMoods.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
        {
            var mid = await GetOrCreateMoodIdAsync(db, sm);
            if (mid == null) continue; // ignore unknown secondary moods
            await db.InsertAsync(new EntryMoodEntity
            {
                EntryID = entity.EntryID,
                MoodID = mid.Value,
                MoodType = "Secondary"
            });
        }

        // Insert Tags
        foreach (var tag in entry.Tags.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
        {
            var tagId = await GetOrCreateTagIdAsync(db, tag);
            await db.InsertAsync(new EntryTagEntity
            {
                EntryID = entity.EntryID,
                TagID = tagId
            });
        }

        // Return final UI model
        var created = await GetEntryByIdAsync(entity.EntryID);
        return (true, "Entry created successfully.", created);
    }

    public async Task<(bool Success, string Message)> UpdateEntryAsync(JournalEntry entry)
    {
        var db = await AppDatabase.GetDatabaseAsync();

        if (entry.Id <= 0) return (false, "Invalid entry.");
        if (string.IsNullOrWhiteSpace(entry.Title)) return (false, "Title is required.");
        if (string.IsNullOrWhiteSpace(entry.PrimaryMood)) return (false, "Primary mood is required.");
        if (entry.SecondaryMoods.Count > 2) return (false, "Maximum 2 secondary moods allowed.");
        
        var existing = await db.Table<JournalEntryEntity>()
            .Where(e => e.EntryID == entry.Id)
            .FirstOrDefaultAsync();

        if (existing == null) return (false, "Entry not found.");
        
        // Check if date is being changed and if another entry exists for the new date
        var newDate = ToDateOnly(entry.Date);
        if (existing.EntryDate != newDate)
        {
            var conflictingEntry = await db.Table<JournalEntryEntity>()
                .Where(e => e.UserID == existing.UserID && e.EntryDate == newDate && e.EntryID != entry.Id)
                .FirstOrDefaultAsync();
            
            if (conflictingEntry != null)
                return (false, "An entry already exists for this date.");
        }
        
        existing.Title = entry.Title.Trim();
        existing.Content = entry.Content ?? "";
        existing.EntryDate = newDate;
        existing.UpdatedAt = DateTime.Now;
        
        var categoryId = await GetOrCreateCategoryIdAsync(db, entry.Category);
        existing.CategoryID = categoryId;
        
        await db.UpdateAsync(existing);

        // Replace moods
        var oldMoods = await db.Table<EntryMoodEntity>()
            .Where(em => em.EntryID == entry.Id)
            .ToListAsync();
        foreach (var em in oldMoods)
            await db.DeleteAsync(em);

        var primaryMoodId = await GetOrCreateMoodIdAsync(db, entry.PrimaryMood.Trim());
        if (primaryMoodId == null) return (false, $"Unknown primary mood: {entry.PrimaryMood}");

        await db.InsertAsync(new EntryMoodEntity
        {
            EntryID = existing.EntryID,
            MoodID = primaryMoodId.Value,
            MoodType = "Primary"
        });

        foreach (var sm in entry.SecondaryMoods.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
        {
            var mid = await GetOrCreateMoodIdAsync(db, sm);
            if (mid == null) continue;
            
            await db.InsertAsync(new EntryMoodEntity
            {
                EntryID = existing.EntryID,
                MoodID = mid.Value,
                MoodType = "Secondary"
            });
        }

        // Replace tags
        var oldTags = await db.Table<EntryTagEntity>()
            .Where(et => et.EntryID == entry.Id)
            .ToListAsync();
        foreach (var et in oldTags)
            await db.DeleteAsync(et);

        foreach (var tag in entry.Tags.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
        {
            var tagId = await GetOrCreateTagIdAsync(db, tag);
            await db.InsertAsync(new EntryTagEntity
            {
                EntryID = existing.EntryID,
                TagID = tagId
            });
        }

        return (true, "Entry updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteEntryAsync(int id)
    {
        var db = await DbAsync();

        var existing = await db.Table<JournalEntryEntity>()
            .Where(e => e.EntryID == id)
            .FirstOrDefaultAsync();

        if (existing == null) return (false, "Entry not found.");

        // Delete junctions first (safe even if FK not enforced)
        var moods = await db.Table<EntryMoodEntity>().Where(x => x.EntryID == id).ToListAsync();
        foreach (var m in moods) await db.DeleteAsync(m);

        var tags = await db.Table<EntryTagEntity>().Where(x => x.EntryID == id).ToListAsync();
        foreach (var t in tags) await db.DeleteAsync(t);

        await db.DeleteAsync(existing);

        return (true, "Entry deleted successfully.");
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        var db = await DbAsync();
        var tags = await db.Table<TagEntity>()
            .OrderBy(t => t.TagName)
            .ToListAsync();
        
        return tags.Select(t => t.TagName).ToList();
    }
    }