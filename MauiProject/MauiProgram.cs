namespace MauiProject;

using DotNet.Meteor.HotReload.Plugin;
using MauiProject.Services;
using MauiProject.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        builder.Services.AddTransient<MainPage>();
		builder.Services.AddSingleton<WeatherViewModel>();
        builder.Services.AddSingleton<ApiService>();

#if DEBUG
		builder.Logging.AddDebug();
		builder.EnableHotReload();

#endif

		return builder.Build();
	}
}
