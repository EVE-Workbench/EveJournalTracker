using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using EWB_Tracker.ViewModels;
using EWB_Tracker.Views;
using EWB_Tracker.Views.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Cache;
using SharedLibrary.Data;
using SharedLibrary.Jobs;
using SharedLibrary.Repositories;
using SharedLibrary.Repositories.Interfaces;
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
        private ClipboardMonitorService _clipboardMonitor;
        private GlobalHotkeyMonitorService _hotkeyMonitorService; 

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
                    

                    #region Views
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<LogView>();
                    services.AddSingleton<DefaultView>();
                    services.AddSingleton<DungeonView>();
                    services.AddSingleton<AccountView>();
                    services.AddSingleton<SettingsView>();
                    
                    services.AddTransient<BountyRunView>();
                    services.AddTransient<DpsChart>();
                    #endregion
                    
                    #region Services
                    services.AddTransient<HttpClient>();
                    services.AddTransient<EwbApiClientService>();
                    services.AddTransient<StartupService>();
                    services.AddTransient<CharacterService>();
                    
                    services.AddSingleton<ClipboardMonitorService>();
                    services.AddSingleton<ClipboardHandlerService>();
                    services.AddSingleton<GlobalHotkeyMonitorService>(); 
                    services.AddSingleton<GlobalHotkeyHandlerService>(); 
                    #endregion

                    #region ViewModels
                    // page view models
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<LogViewModel>();
                    services.AddSingleton<AccountViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                    
                    // misc view models
                    services.AddTransient<DungeonViewModel>();
                    services.AddTransient<BountyRunViewModel>();
                    services.AddTransient<DpsChartViewModel>();
                    #endregion
                    
                    #region Repositories
                    services.AddTransient<IDungeonRepository, DungeonRepository>();
                    services.AddTransient<ISettingRepository, SettingRepository>();
                    #endregion

                    services.AddSingleton<CharacterCache>();
                    services.AddSingleton(provider =>
                    {
                        var logFolderLocation = EveUtils.GetDefaultLogFolderLocation();
                        var characterCache = provider.GetRequiredService<CharacterCache>();
                        var context = provider.GetRequiredService<AppDbContext>();
                        var characterService = provider.GetRequiredService<CharacterService>();

                        return new FileWatcherService(logFolderLocation, characterCache, characterService, context);
                    });

                    services.AddSingleton(provider =>
                    {
                        var characterCache = provider.GetRequiredService<CharacterCache>();
                        var characterService = provider.GetRequiredService<CharacterService>();
                        return new CheckOnlineJob(5000, characterCache, characterService);
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
            Task.Run(async () => await initService.Initialize()).Wait();

            try
            {
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
                
                base.OnStartup(e);

                var watcherService = _host.Services.GetRequiredService<FileWatcherService>();
                watcherService.StartWatching();

                _checkOnlineJob = _host.Services.GetRequiredService<CheckOnlineJob>();
                _checkOnlineJob.Start();
            
                StartClipboardMonitoring(mainWindow);
                StartHotkeyMonitoring(mainWindow);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while starting main window: {ex.Message}");
                MessageBox.Show("An error occurred while starting the application. Please check the logs for more details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        
        private void StartClipboardMonitoring(MainWindow mainWindow)
        {
            _clipboardMonitor = _host.Services.GetRequiredService<ClipboardMonitorService>();
            var clipboardHandler = _host.Services.GetRequiredService<ClipboardHandlerService>();

            // Subscribe to clipboard changes
            _clipboardMonitor.ClipboardChanged += async (content) =>
            {
                try
                {
                    await clipboardHandler.ProcessClipboardContent(content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing clipboard content: {ex.Message}");
                }
            };

            // Start monitoring
            _clipboardMonitor.StartMonitoring(mainWindow);
        }

        private void StartHotkeyMonitoring(MainWindow mainWindow)
        {
            _hotkeyMonitorService = _host.Services.GetRequiredService<GlobalHotkeyMonitorService>();
            
            // Initialize the service with the main window
            _hotkeyMonitorService.Initialize(mainWindow);
            
            var hotkeyHandler = _host.Services.GetRequiredService<GlobalHotkeyHandlerService>();
            hotkeyHandler.Initialize(_hotkeyMonitorService);
            
            Console.WriteLine("Hotkey monitoring started");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var watcherService = _host.Services.GetRequiredService<FileWatcherService>();
            watcherService.StopWatching();

            _checkOnlineJob.Stop();
            
            // Stop clipboard monitoring
            _clipboardMonitor?.StopMonitoring();
            _clipboardMonitor?.Dispose();

            // Stop hotkey monitoring
            _hotkeyMonitorService?.Dispose();
            
            base.OnExit(e);
        }
    }
}