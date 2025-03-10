using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using MonitorSaude.Interfaces;
using MonitorSaude.Mappers;
using MonitorSaude.Services;
using MonitorSaude.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI;

namespace MonitorSaude;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMicrocharts()
            .UseSkiaSharp()
            .UseUraniumUI()
			.UseUraniumUIMaterial()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFontAwesomeIconFonts();
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<IGoogleFitService, GoogleFitService>();
        builder.Services.AddSingleton<IUserProfileMapper, UserProfileMapper>();
        builder.Services.AddSingleton<IGoogleAuthService, GoogleAuthService>();
        builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
        builder.Services.AddSingleton<ISyncService, SyncService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<NutritionViewModel>();

        return builder.Build();

	}
}
