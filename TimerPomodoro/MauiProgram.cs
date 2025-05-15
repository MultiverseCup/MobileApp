using Microsoft.Extensions.Logging;
using TimerPomodoro.ViewModel;

namespace TimerPomodoro;

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

        builder.Services.AddSingleton<TimerPage>();
        builder.Services.AddSingleton<TimerViewModel>();

        builder.Services.AddSingleton<PurposesPage>();
        builder.Services.AddSingleton<PurposesViewModel>();

        builder.Services.AddSingleton<ShedulePage>();
        builder.Services.AddSingleton<SheduleViewModel>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
