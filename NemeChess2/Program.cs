using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace NemeChess2
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<LichessApiService>();
        }
    }
}
