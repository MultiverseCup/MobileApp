using PomodoroProject.Data;
using PomodoroProject.ViewModels;

using Microsoft.Extensions.Logging;
using PomodoroProject.Views;

namespace PomodoroProject;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("CustomFont.ttf", "CustomFont");
        });

        builder.Services.AddSingleton<AppDatabase>(sp =>
    new AppDatabase(Path.Combine(
        FileSystem.AppDataDirectory,
        "pomodoro.db3")));

        builder.Services.AddSingleton<TimerViewModel>();
        builder.Services.AddSingleton<TimerPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
