using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedLibrary.Services;
using UI.avalonia.ViewModels;
using UI.avalonia.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UI.avalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly FileWatcherService _fileWatcherService;
        private readonly GlobalHotkeyService _globalHotkeyService;
        private bool _isWatching = true;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(MainWindowViewModel viewModel, FileWatcherService fileWatcherService, GlobalHotkeyService globalHotkeyService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = viewModel;

            _fileWatcherService = fileWatcherService;
            _globalHotkeyService = globalHotkeyService;
            _serviceProvider = serviceProvider;
            viewModel.CurrentView = _serviceProvider.GetService<DefaultView>();

            _fileWatcherService.OnISKUpdated += async (sender, e) =>
            {
                viewModel.UpdateCurrentBountyRunIsk(e.ISKChange, e.Character);
            };

            Opened += (s, e) =>
            {
                _fileWatcherService.StartWatching();
                InitializeGlobalHotkeys();
            };

            Closing += (s, e) =>
            {
                _globalHotkeyService.Dispose();
                // Releases the file watcher, background services and host, then exits.
                (App.Current as App)?.Shutdown();
            };

            // Keep local keyboard event handler as fallback
            KeyDown += Window_KeyDown;
        }

        private void InitializeGlobalHotkeys()
        {
            try
            {
                // Initialize the global hotkey service with this window
                _globalHotkeyService.Initialize(this);

                // Register hotkeys
                _globalHotkeyService.RegisterHotkey("NewBountyRun",
                    GlobalHotkeyService.ModifierKeys.Control | GlobalHotkeyService.ModifierKeys.Shift,
                    Key.N);

                _globalHotkeyService.RegisterHotkey("OpenJournal",
                    GlobalHotkeyService.ModifierKeys.Control | GlobalHotkeyService.ModifierKeys.Shift,
                    Key.J);

                // Subscribe to hotkey events
                _globalHotkeyService.HotkeyPressed += OnGlobalHotkeyPressed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing global hotkeys: {ex.Message}");
            }
        }

        private void OnGlobalHotkeyPressed(int hotkeyId, string hotkeyName)
        {
            // Execute on UI thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                switch (hotkeyName)
                {
                    case "NewBountyRun":
                        // Start new bounty run directly without modal
                        StartNewBountyRunDirect();
                        break;

                    case "OpenJournal":
                        OpenEveJournal_Click(this, new RoutedEventArgs());
                        break;
                }
            });
        }

        private void StartNewBountyRunDirect()
        {
            var mainViewModel = (MainWindowViewModel)DataContext;

            // Generate default name with current time
            var runCount = mainViewModel.BountyRuns.Count + 1;
            var currentTime = DateTime.Now.ToString("h:mm tt");
            var runName = $"Run #{runCount}, {currentTime}";

            var bountyRun = new SharedLibrary.Models.BountyRun
            {
                Id = DateTime.Now.Ticks.GetHashCode(),
                Name = runName,
                StartTime = DateTime.Now,
                TotalIsk = 0,
                IsCompleted = false
            };

            mainViewModel.SetCurrentBountyRun(bountyRun);
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            // Check for Ctrl+Shift+N (Start new bounty run)
            if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift) && e.Key == Key.N)
            {
                e.Handled = true;
                StartNewBountyRunDirect();
            }
            // Check for Ctrl+Shift+J (Open EVE Journal)
            else if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift) && e.Key == Key.J)
            {
                e.Handled = true;
                OpenEveJournal_Click(this, new RoutedEventArgs());
            }
        }

        public void CloseModal()
        {
            ModalOverlay.IsVisible = false;
            ModalContent.Content = null;
        }

        private void BountyRunButton_Click(object sender, RoutedEventArgs e)
        {
            var mainViewModel = (MainWindowViewModel)DataContext;

            if (mainViewModel.IsBountyRunActive)
            {
                // Stop current bounty run
                mainViewModel.StopCurrentBountyRun();
            }
            else
            {
                // Start new bounty run
                var view = _serviceProvider.GetService<BountyRunView>();
                ModalContent.Content = view;
                ModalOverlay.IsVisible = true;
            }
        }

        private void ModalOverlay_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            var point = e.GetPosition(ModalContent);
            var bounds = ModalContent.Bounds;

            // Check if click is outside modal content
            if (point.X < 0 || point.Y < 0 || point.X > bounds.Width || point.Y > bounds.Height)
            {
                CloseModal();
            }
        }

        private void StopWatching_Click(object sender, RoutedEventArgs e)
        {
            if (_isWatching)
            {
                _fileWatcherService.StopWatching();
                ((Button)sender).Content = "Start Watching";
                // Note: In Avalonia, use a proper dialog system instead of MessageBox
                // For now, we'll just update the button
            }
            else
            {
                _fileWatcherService.StartWatching();
                ((Button)sender).Content = "Stop Watching";
            }

            _isWatching = !_isWatching;
        }

        // Minimize Window
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Maximize or Restore Window
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        // Close Window
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Handle Pointer Press on Title Bar
        private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void OpenEveJournal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = App.ServiceProvider.GetService<IConfiguration>().GetValue<string>("EveJournalUrl");
                if (!string.IsNullOrEmpty(url))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening EVE Journal: {ex.Message}");
            }
        }
    }
}
