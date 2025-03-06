using System;
using System.Threading;
using System.Windows;
using EWB_Tracker.ViewModels;
using EWB_Tracker.Views;
using Microsoft.EntityFrameworkCore;
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
    public partial class App : Application
    {
        private readonly IHost _host;
        private CheckOnlineJob _checkOnlineJob;
        
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<AppDbContext>();

                    services.AddHostedService<BackgroundSaveService>();

                    services.AddTransient<MainWindow>();
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<LogView>();
                    services.AddTransient<DefaultView>();

                    services.AddSingleton<CharacterCache>();
                    services.AddSingleton<FileWatcherService>(provider =>
                    {
                        var logFolderLocation = EveUtils.GetDefaultLogFolderLocation();
                        var characterCache = provider.GetRequiredService<CharacterCache>();

                        return new FileWatcherService(logFolderLocation, characterCache);
                    });
                    
                    services.AddSingleton<CheckOnlineJob>(provider =>
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

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();


            base.OnStartup(e);

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