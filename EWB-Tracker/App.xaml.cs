using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using EWB_Tracker.ViewModels;
using EWB_Tracker.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Cache;
using SharedLibrary.Data;
using SharedLibrary.Jobs;
using SharedLibrary.Services;
using SharedLibrary.Utils;

namespace EWB_Tracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly IHost _host;
        private CheckOnlineJob _checkOnlineJob;

        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        public App()
        {
            var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(Configuration);
                    
                    services.AddDbContext<AppDbContext>();

                    services.AddHostedService<BackgroundSaveService>();
                    

                    services.AddTransient<MainWindow>();
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<LogView>();
                    services.AddTransient<DefaultView>();
                    
                    services.AddTransient<HttpClient>();
                    services.AddTransient<EwbApiClientService>();
                    services.AddTransient<StartupService>();
                    
                    services.AddSingleton<CharacterCache>();
                    services.AddSingleton(provider =>
                    {
                        var logFolderLocation = EveUtils.GetDefaultLogFolderLocation();
                        var characterCache = provider.GetRequiredService<CharacterCache>();
                        var context = provider.GetRequiredService<AppDbContext>();

                        return new FileWatcherService(logFolderLocation, characterCache, context);
                    });

                    services.AddSingleton(provider =>
                    {
                        var characterCache = provider.GetRequiredService<CharacterCache>();
                        return new CheckOnlineJob(5000, characterCache);
                    });
                })
                .Build();

            ServiceProvider = _host.Services;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    var databaseExists = context.Database.CanConnect();

                    if (!databaseExists)
                    {
                        Console.WriteLine("Database does not exist yet, creating database...");
                    }

                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while migrating database: {ex.Message}");
                }
            }
            
            _host.Start();
            
            var initService = _host.Services.GetRequiredService<StartupService>();
            initService.Initialize();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();


            base.OnStartup(e);

            var dbContext = _host.Services.GetRequiredService<AppDbContext>();
            var watcherService = _host.Services.GetRequiredService<FileWatcherService>();
            watcherService.StartWatching();

            _checkOnlineJob = _host.Services.GetRequiredService<CheckOnlineJob>();
            _checkOnlineJob.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var watcherService = _host.Services.GetRequiredService<FileWatcherService>();
            watcherService.StopWatching();

            _checkOnlineJob.Stop();
            base.OnExit(e);
        }
    }
}