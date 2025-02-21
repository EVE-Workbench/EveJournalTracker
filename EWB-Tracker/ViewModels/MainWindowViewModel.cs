using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using EWB_Tracker.Commands;
using EWB_Tracker.Views;
using SharedLibrary.Models;
using SharedLibrary.Repositories;
using SharedLibrary.Services;

namespace EWB_Tracker.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
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
        public ObservableCollection<SystemBounty> Systems { get; set; }
        public int TotalIsk { get; set; } = 0;
        public int IskChange { get; set; } = 0;
        
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

        public MainWindowViewModel()
        {
            _characters = new ObservableCollection<Character>(CharacterRepository.Instance.Characters.Values);
            CharacterRepository.Instance.CharacterAdded += OnCharacterAdded;
            
            Sprints = new ObservableCollection<Sprint>();
            Systems = new ObservableCollection<SystemBounty>();
            
            CurrentView = new DefaultView(); 

            ShowViewCommand = new RelayCommand(ShowView);
            
            var fileWatcherService = ServiceLocator.GetService<FileWatcherService>();
            fileWatcherService.OnISKUpdated += (sender, iskValues) =>
            {
                TotalIsk = iskValues.TotalISK;
                IskChange = iskValues.ISKChange;
                OnPropertyChanged(nameof(TotalIsk));
                OnPropertyChanged(nameof(IskChange));
            };
        }
        
        private void OnCharacterAdded(Character newCharacter)
        {
            App.Current.Dispatcher.Invoke(() => Characters.Add(newCharacter));
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
