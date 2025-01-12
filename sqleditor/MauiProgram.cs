using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MauiIcons.Material;

namespace sqleditor
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() 
                .UseMaterialMauiIcons()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Geist-Regular.ttf", "GeistRegular");
                    fonts.AddFont("Geist-SemiBold.ttf", "GeistSemiBold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
