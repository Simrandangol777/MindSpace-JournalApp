namespace Mindspace.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    event Action? OnThemeChanged;

    Task InitializeAsync();
    Task SetThemeAsync(string theme);
}