using PomodoroProject.Data;
using PomodoroProject.ViewModels;
using Microsoft.Extensions.Logging;
using PomodoroProject.Views;
using CommunityToolkit.Maui; // Добавьте это для использования .UseMauiCommunityToolkit()
using Plugin.LocalNotification;

namespace PomodoroProject;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .UseMauiCommunityToolkit() // Добавлено: должно быть сразу после UseMauiApp
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("CustomFont.ttf", "CustomFont"); // Объединены шрифты
            });

        builder.Services.AddSingleton<AppDatabase>(sp =>
            new AppDatabase(Path.Combine(
                FileSystem.AppDataDirectory,
                "pomodoro.db3")));

       

        builder.Services.AddSingleton<TimerViewModel>( );
        builder.Services.AddSingleton<TimerPage>();

        builder.Services.AddSingleton<PurposesViewModel>();
        builder.Services.AddSingleton<PurposesPage>();

        builder.Services.AddSingleton<ScheduleViewModel>();
        builder.Services.AddSingleton<SchedulePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}