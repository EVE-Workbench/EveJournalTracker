using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedLibrary.Cache;
using SharedLibrary.Data;
using SharedLibrary.Jobs;
using SharedLibrary.Repositories;
using SharedLibrary.Repositories.Interfaces;
using SharedLibrary.Services;
using SharedLibrary.Utils;
using UI.avalonia.ViewModels;
using UI.avalonia.Views;
using UI.avalonia.Views.Components;

namespace UI.avalonia;

public partial class App : Application
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
                services.AddSingleton<UI.avalonia.Services.GlobalHotkeyService>();
                services.AddSingleton<UI.avalonia.Services.ShortcutService>();
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
                    var characterCache = provider.GetRequiredService<CharacterCache>();
                    var context = provider.GetRequiredService<AppDbContext>();
                    var characterService = provider.GetRequiredService<CharacterService>();
                    var settingsRepository = provider.GetRequiredService<ISettingRepository>();

                    var logDirSetting = settingsRepository.GetByKeyAsync("LogDir").GetAwaiter().GetResult();
                    var logFolderLocation = ResolveLogFolder(logDirSetting?.Value, settingsRepository);
                    var logger = provider.GetRequiredService<ILogger<FileWatcherService>>();

                    return new FileWatcherService(logFolderLocation, characterCache, characterService, context, logger);
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

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Initialize database
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    // Carry over a database from older versions that stored it next to the exe.
                    SharedLibrary.Utils.AppPaths.MigrateLegacyDatabase();

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
                desktop.MainWindow = mainWindow;
                mainWindow.Show();

                // The watcher is started from MainWindow.Opened; starting it here too would
                // attach a second FileSystemWatcher and double-count every log event.

                _checkOnlineJob = _host.Services.GetRequiredService<CheckOnlineJob>();
                _checkOnlineJob.Start();

                // Note: ClipboardMonitoring and HotkeyMonitoring are WPF-specific and disabled in Avalonia
                // These features can be re-implemented using Avalonia-specific approaches if needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while starting main window: {ex.Message}");
                // Note: In Avalonia, we'll handle message boxes differently in the views
                return;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private bool _isShuttingDown;

    public void Shutdown()
    {
        if (_isShuttingDown)
            return;
        _isShuttingDown = true;

        try
        {
            // Flush any pending character changes before tearing the host down.
            _host.Services.GetRequiredService<CharacterCache>().SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during final save: {ex.Message}");
        }

        try { _host.Services.GetRequiredService<FileWatcherService>().Dispose(); } catch { }
        try { _checkOnlineJob?.Stop(); } catch { }
        try { _host.StopAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult(); } catch { }
        _host.Dispose();

        // Avalonia's X11/D-Bus teardown can race on Linux and surface a benign
        // TaskCanceledException as an unhandled exception on exit. Our own resources are
        // released above, so exit promptly to avoid that noisy crash during shutdown.
        Environment.Exit(0);
    }

    private static string ResolveLogFolder(string? savedPath, ISettingRepository settingsRepository)
    {
        if (!string.IsNullOrWhiteSpace(savedPath) && Directory.Exists(savedPath))
            return savedPath;

        // Saved path is empty or no longer exists (e.g. EVE now runs through a different
        // Steam/Proton prefix). Auto-detect, and persist the correction when we find a real
        // folder so it sticks and shows up in settings.
        var detected = EveLogLocator.Detect();
        if (Directory.Exists(detected) && !string.Equals(detected, savedPath, StringComparison.Ordinal))
            settingsRepository.UpsertAsync("LogDir", detected).GetAwaiter().GetResult();

        return detected;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}