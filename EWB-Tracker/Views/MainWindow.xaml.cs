using EWB_Tracker.ViewModels;
using System.Windows;
using SharedLibrary.Services;

namespace EWB_Tracker
{
    public partial class MainWindow : Window
    {
        
        private FileWatcherService _fileWatcherService;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            
            _fileWatcherService = ServiceLocator.GetService<FileWatcherService>();
            
            Loaded += (s, e) => _fileWatcherService.StartWatching();
            Closing += (s, e) => _fileWatcherService.StopWatching();
        }

        private void StartWatching_Click(object sender, RoutedEventArgs e)
        {
            _fileWatcherService.StartWatching();
            MessageBox.Show("Started watching log files.");
        }

        private void StopWatching_Click(object sender, RoutedEventArgs e)
        {
            _fileWatcherService.StopWatching();
            MessageBox.Show("Stopped watching log files.");
        }
    }
}