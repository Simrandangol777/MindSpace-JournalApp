using Mindspace.Models;

namespace Mindspace.Services;

public class DashboardService : IDashboardService
    {
        private readonly IJournalService _journalService;

        public DashboardService(IJournalService journalService)
        {
            _journalService = journalService;
        }
        
        public async Task<List<JournalEntry>> GetRecentEntriesAsync(int userId, int count)
        {
            var entries = await _journalService.GetAllEntriesAsync(userId);
            return entries
                .OrderByDescending(e => e.Date)
                .Take(count)
                .ToList();
        }


        public async Task<DashboardStats> GetStatsAsync(int userId)
        {
            var entries = await _journalService.GetAllEntriesAsync(userId);
            
            var stats = new DashboardStats
            {
                TotalEntries = entries.Count,
                CurrentStreak = CalculateCurrentStreak(entries),
                LongestStreak = CalculateLongestStreak(entries),
                MissedDays = CalculateMissedDays(entries),
                MoodDistribution = CalculateMoodDistribution(entries),
                MoodFrequency = CalculateMoodFrequency(entries),
                TagUsage = CalculateTagUsage(entries),
                AverageWordCount = entries.Any() ? entries.Average(e => e.WordCount) : 0
            };

            return stats;
        }

        private int CalculateCurrentStreak(List<JournalEntry> entries)
        {
            if (!entries.Any()) return 0;

            var sortedDates = entries.Select(e => e.Date.Date).Distinct().OrderByDescending(d => d).ToList();
            var today = DateTime.Today;
            
            // Check if there's an entry today or yesterday
            if (sortedDates.First() != today && sortedDates.First() != today.AddDays(-1))
                return 0;

            int streak = 0;
            var currentDate = sortedDates.First();
            
            foreach (var date in sortedDates)
            {
                if (date == currentDate)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else if (date < currentDate)
                {
                    break;
                }
            }

            return streak;
        }

        private int CalculateLongestStreak(List<JournalEntry> entries)
        {
            if (!entries.Any()) return 0;

            var sortedDates = entries.Select(e => e.Date.Date).Distinct().OrderBy(d => d).ToList();
            
            int longestStreak = 1;
            int currentStreak = 1;
            
            for (int i = 1; i < sortedDates.Count; i++)
            {
                if ((sortedDates[i] - sortedDates[i - 1]).Days == 1)
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }

            return longestStreak;
        }

        private int CalculateMissedDays(List<JournalEntry> entries)
        {
            if (!entries.Any()) return 0;

            var dates = entries.Select(e => e.Date.Date).Distinct().ToList();
            var start = dates.Min();
            // var end = DateTime.Today;
            var end = dates.Max();

            var totalDays = (end - start).Days + 1;
            var missed = totalDays - dates.Count;

            return Math.Max(0, missed);
        }

        private Dictionary<string, int> CalculateMoodDistribution(List<JournalEntry> entries)
        {
            var distribution = new Dictionary<string, int>
            {
                ["Positive"] = 0,
                ["Neutral"] = 0,
                ["Negative"] = 0
            };

            var positiveMoods = new[] { "Happy", "Excited", "Relaxed", "Grateful", "Confident" };
            var neutralMoods = new[] { "Calm", "Thoughtful", "Curious", "Nostalgic", "Bored" };
            var negativeMoods = new[] { "Sad", "Angry", "Stressed", "Lonely", "Anxious" };

            foreach (var entry in entries)
            {
                if (positiveMoods.Contains(entry.PrimaryMood))
                    distribution["Positive"]++;
                else if (neutralMoods.Contains(entry.PrimaryMood))
                    distribution["Neutral"]++;
                else if (negativeMoods.Contains(entry.PrimaryMood))
                    distribution["Negative"]++;
            }

            return distribution;
        }

        private Dictionary<string, int> CalculateMoodFrequency(List<JournalEntry> entries)
        {
            return entries
                .GroupBy(e => e.PrimaryMood)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private Dictionary<string, int> CalculateTagUsage(List<JournalEntry> entries)
        {
            return entries
                .SelectMany(e => e.Tags)
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }