using Microsoft.Extensions.Logging;
using Mindspace.Services;

namespace Mindspace;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IJournalService, JournalService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IExportService, ExportService>();
        builder.Services.AddSingleton<IPinLockService, PinLockService>();
        
        var app = builder.Build();

        var auth = app.Services.GetRequiredService<IAuthService>();
        var pin = app.Services.GetRequiredService<IPinLockService>();

        Task.Run(async () =>
        {
            await auth.TryRestoreSessionAsync();
            await pin.LockAsync();
        });
        return app;
    }
}