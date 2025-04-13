using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharedLibrary.Services;
using EWB_Tracker.ViewModels;
using EWB_Tracker.Views;

namespace EWB_Tracker
{
    public partial class MainWindow : Window
    {
        private readonly FileWatcherService _fileWatcherService;
        private bool _isWatching = true;

        public MainWindow(MainWindowViewModel viewModel, FileWatcherService fileWatcherService)
        {
            InitializeComponent();
            DataContext = viewModel;

            _fileWatcherService = fileWatcherService;

            Loaded += (s, e) => _fileWatcherService.StartWatching();
            Closing += (s, e) => _fileWatcherService.StopWatching();
        }
        
        private void CloseModal()
        {
            ModalOverlay.Visibility = Visibility.Collapsed;
            ModalContent.Content = null;
        }

        private void DungeonWindow_Click(Object sender, RoutedEventArgs e)
        {
            var view = new DungeonView(); 
            ModalContent.Content = view;
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void ModalOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not FrameworkElement element || !ModalContent.IsAncestorOf(element))
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
                MessageBox.Show("Stopped watching log files.");
            }
            else
            {
                _fileWatcherService.StartWatching();
                ((Button)sender).Content = "Stop Watching";
                MessageBox.Show("Started watching log files.");
            }

            _isWatching = !_isWatching;
        }

        // Minimize Window
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Maximize or Restore Window
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // Close Window
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Handle Mouse Left Button Down on Title Bar
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
            }
            else
            {
                this.DragMove();
            }
        }
    }
}
