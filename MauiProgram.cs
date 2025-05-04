using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using Plugin.AdMob;
using Plugin.AdMob.Configuration;

namespace TextToImageGonfyUiV2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseAdMob()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.UseMauiApp<App>().UseMauiCommunityToolkitCore();

            AdConfig.UseTestAdUnitIds = true; // Use test ad unit IDs. Setwa testowi reklami
            AdConfig.DisableConsentCheck = true; // Disable consent check.     

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
