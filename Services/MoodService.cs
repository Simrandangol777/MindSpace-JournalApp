using Mindspace.Models;

namespace Mindspace.Services;

public class MoodService
{
    public static List<Mood> GetAllMoods()
    {
        return new List<Mood>
        {
            // Positive Moods
            new Mood { Id = 1, Name = "Happy", Emoji = "😊", Category = "Positive" },
            new Mood { Id = 2, Name = "Excited", Emoji = "🤩", Category = "Positive" },
            new Mood { Id = 3, Name = "Relaxed", Emoji = "😌", Category = "Positive" },
            new Mood { Id = 4, Name = "Grateful", Emoji = "🙏", Category = "Positive" },
            new Mood { Id = 5, Name = "Confident", Emoji = "💪", Category = "Positive" },
                
            // Neutral Moods
            new Mood { Id = 6, Name = "Calm", Emoji = "😐", Category = "Neutral" },
            new Mood { Id = 7, Name = "Thoughtful", Emoji = "🤔", Category = "Neutral" },
            new Mood { Id = 8, Name = "Curious", Emoji = "🧐", Category = "Neutral" },
            new Mood { Id = 9, Name = "Nostalgic", Emoji = "💭", Category = "Neutral" },
            new Mood { Id = 10, Name = "Bored", Emoji = "😑", Category = "Neutral" },
                
            // Negative Moods
            new Mood { Id = 11, Name = "Sad", Emoji = "😢", Category = "Negative" },
            new Mood { Id = 12, Name = "Angry", Emoji = "😠", Category = "Negative" },
            new Mood { Id = 13, Name = "Stressed", Emoji = "😰", Category = "Negative" },
            new Mood { Id = 14, Name = "Lonely", Emoji = "😔", Category = "Negative" },
            new Mood { Id = 15, Name = "Anxious", Emoji = "😟", Category = "Negative" }
        };
    }
}