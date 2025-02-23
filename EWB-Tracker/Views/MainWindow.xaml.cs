using System;
using EWB_Tracker.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharedLibrary.Services;

namespace EWB_Tracker
{
    public partial class MainWindow : Window
    {
        private FileWatcherService _fileWatcherService;
        private bool _isWatching = true;
        private bool isDragging = false;
        private Point startPoint;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            _fileWatcherService = ServiceLocator.GetService<FileWatcherService>();

            Loaded += (s, e) => _fileWatcherService.StartWatching();
            Closing += (s, e) => _fileWatcherService.StopWatching();
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
        
        // Slepen en Dubbelklik afhandelen
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Dubbelklik om te Maximaliseren of Restoren
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
                // Enkelklik om te slepen
                this.DragMove();
            }
        }
    }
}