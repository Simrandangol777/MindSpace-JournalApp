namespace Mindspace.Services;

public class ThemeService : IThemeService
{
    private const string Key = "mindspace_theme";
    private string _currentTheme = "custom";

    public string CurrentTheme => _currentTheme;

    public event Action? OnThemeChanged;

    public Task InitializeAsync()
    {
        _currentTheme = Preferences.Get(Key, "custom");
        OnThemeChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task SetThemeAsync(string theme)
    {
        theme = (theme ?? "custom").ToLowerInvariant();
        if (theme is not ("light" or "dark" or "custom"))
            theme = "custom";

        _currentTheme = theme;
        Preferences.Set(Key, theme);
        OnThemeChanged?.Invoke();
        return Task.CompletedTask;
    }
}