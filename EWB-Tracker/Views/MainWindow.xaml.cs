using EWB_Tracker.ViewModels;
using System.Windows;
using System.Windows.Controls;
using SharedLibrary.Services;

namespace EWB_Tracker
{
    public partial class MainWindow : Window
    {
        
        private FileWatcherService _fileWatcherService;
        private bool _isWatching = true;
        
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
    }
}