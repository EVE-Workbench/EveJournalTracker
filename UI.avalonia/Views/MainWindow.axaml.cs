using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedLibrary.Services;
using UI.avalonia.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UI.avalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly FileWatcherService _fileWatcherService;
        private bool _isWatching = true;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(MainWindowViewModel viewModel, FileWatcherService fileWatcherService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = viewModel;

            _fileWatcherService = fileWatcherService;
            _serviceProvider = serviceProvider;
            viewModel.CurrentView = _serviceProvider.GetService<DefaultView>();

            _fileWatcherService.OnISKUpdated += async (sender, e) =>
            {
                viewModel.UpdateCurrentBountyRunIsk(e.ISKChange, e.Character);
            };

            Opened += (s, e) => _fileWatcherService.StartWatching();
            Closing += (s, e) =>
            {
                _fileWatcherService.StopWatching();
                // Call the app shutdown method
                (App.Current as App)?.Shutdown();
            };
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
