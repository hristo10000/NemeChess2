using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

namespace NemeChess2
{
    public class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                var services = new ServiceCollection();
                ConfigureServices(services, configuration);

                services.AddSingleton<LichessApiService>();

                services.AddTransient<MainWindow>();

                ServiceProvider = services.BuildServiceProvider();

                desktop.MainWindow = new MainWindow()
                {
                    DataContext = new MainViewModel(ServiceProvider.GetService<LichessApiService>())
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
        private void ConfigureServices(ServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<LichessApiService>(_ => new LichessApiService(configuration));
        }
    }
}
