using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using EWB_Tracker.Commands;
using EWB_Tracker.Views;
using SharedLibrary.Cache;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace EWB_Tracker.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly FileWatcherService _fileWatcherService;
        private readonly CharacterCache _characterCache;
        private object _currentView;
        private ObservableCollection<Character> _characters;

        public ObservableCollection<Character> Characters
        {
            get => _characters;
            set
            {
                _characters = value;
                OnPropertyChanged(nameof(Characters));
            }
        }

        public ObservableCollection<Sprint> Sprints { get; set; }

        private int _totalIsk;
        public int TotalIsk
        {
            get => _totalIsk;
            set
            {
                _totalIsk = value;
                OnPropertyChanged(nameof(TotalIsk));
            }
        }

        private int _iskChange;
        public int IskChange
        {
            get => _iskChange;
            set
            {
                _iskChange = value;
                OnPropertyChanged(nameof(IskChange));
            }
        }

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand ShowViewCommand { get; }

        // FileWatcherService wordt nu via DI geïnjecteerd
        public MainWindowViewModel(FileWatcherService fileWatcherService, CharacterCache characterCache)
        {
            _fileWatcherService = fileWatcherService;
            _characterCache = characterCache;
            _characterCache.CharacterAdded += (sender, e) =>
            {
                var characters = Characters.ToList();
                characters.Add(e);
                Characters = new ObservableCollection<Character>(characters);
            };

            var characters = _characterCache.GetAllCharacters();
            _characters = new ObservableCollection<Character>(characters);

            Sprints = new ObservableCollection<Sprint>();

            CurrentView = new DefaultView(); 

            ShowViewCommand = new RelayCommand(ShowView);

            // Subscriben op ISK updates
            _fileWatcherService.OnISKUpdated += (sender, iskValues) =>
            {
                TotalIsk = iskValues.TotalISK;
                IskChange = iskValues.ISKChange;
            };
        }

        private void ShowView(object view)
        {
            CurrentView = view;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
