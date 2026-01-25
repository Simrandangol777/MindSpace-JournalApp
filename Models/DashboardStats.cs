namespace Mindspace.Models;

public class DashboardStats
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int MissedDays { get; set; }
    public int TotalEntries { get; set; }
    public Dictionary<string, int> MoodDistribution { get; set; } = new();
    public Dictionary<string, int> MoodFrequency { get; set; } = new();
    public Dictionary<string, int> TagUsage { get; set; } = new();
    public double AverageWordCount { get; set; }
}