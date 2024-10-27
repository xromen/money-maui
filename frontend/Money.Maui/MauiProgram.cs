using Microsoft.Extensions.Logging;
using Money.Maui.Services;
using Money.Shared;
using Money.Shared.Interfaces;

namespace Money.Maui
{
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

            builder.Services.AddScoped<IFormFactor, FormFactorService>();
            builder.Services.AddScoped<ILocalStorage, LocalStorageService>();

            builder.Services.AddMauiBlazorWebView();

            builder.Services.ConfigureHybryd();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
