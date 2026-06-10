using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.avalonia.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Cache;
using SharedLibrary.Events;
using SharedLibrary.Jobs;
using SharedLibrary.Models;
using SharedLibrary.Services;
using SharedLibrary.Repositories.Interfaces;

namespace UI.avalonia.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly FileWatcherService _fileWatcherService;
        private readonly CharacterCache _characterCache;
        private readonly CheckOnlineJob _checkOnlineJob;
        private readonly ISettingRepository _settingRepository;
        private readonly IConfiguration _configuration;
        private object _currentView;
        private ObservableCollection<Character> _characters;
        private bool _showOfflineCharacters = false;

        private BountyRun _currentBountyRun;
        private bool _isBountyRunActive = false;

        public ObservableCollection<Character> FilteredCharacters { get; set; }
        public ObservableCollection<BountyRun> BountyRuns { get; set; }
        private ObservableCollection<BountyRun> _topBountyRuns;
        public ObservableCollection<BountyRun> TopBountyRuns
        {
            get => _topBountyRuns;
            private set
            {
                _topBountyRuns = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Character> Characters
        {
            get => _characters;
            set
            {
                _characters = value;
                OnPropertyChanged(nameof(Characters));
                FilterCharacters();
            }
        }

        public BountyRun CurrentBountyRun
        {
            get => _currentBountyRun;
            set
            {
                _currentBountyRun = value;
                OnPropertyChanged();
            }
        }

        public bool IsBountyRunActive
        {
            get => _isBountyRunActive;
            set
            {
                _isBountyRunActive = value;
                OnPropertyChanged();
            }
        }

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
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        // The active view's type name, used to highlight the matching navigation button.
        public string CurrentPage => _currentView?.GetType().Name ?? string.Empty;

        public bool ShowOfflineCharacters
        {
            get => _showOfflineCharacters;
            set
            {
                _showOfflineCharacters = value;
                OnPropertyChanged(nameof(ShowOfflineCharacters));
                FilterCharacters();
            }
        }

        public ICommand ShowViewCommand { get; }

        public MainWindowViewModel(FileWatcherService fileWatcherService, CharacterCache characterCache, CheckOnlineJob checkOnlineJob, ISettingRepository settingRepository, IConfiguration configuration)
        {
            _fileWatcherService = fileWatcherService;
            _characterCache = characterCache;
            _checkOnlineJob = checkOnlineJob;
            _settingRepository = settingRepository;
            _configuration = configuration;

            _characterCache.CharacterAdded += (sender, e) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    WatchCharacter(e);
                    var characters = Characters.ToList();
                    characters.Add(e);
                    Characters = new ObservableCollection<Character>(characters);
                });
            };

            var characters = _characterCache.GetAllCharacters();
            foreach (var character in characters)
                WatchCharacter(character);

            _characters = new ObservableCollection<Character>(characters);
            FilteredCharacters = new ObservableCollection<Character>(_characters);

            BountyRuns = new ObservableCollection<BountyRun>();
            TopBountyRuns = new ObservableCollection<BountyRun>();

            ShowViewCommand = new RelayCommand(ShowView);

            ShowOfflineCharacters = _showOfflineCharacters;

            _fileWatcherService.OnISKUpdated += (sender, update) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    TotalIsk = update.TotalBounty;
                    IskChange = update.LastBounty;
                    update.Character.Bounty = update.CharacterBounty;
                });
            };

            _checkOnlineJob.CharacterStatusChanged += OnCharacterStatusChanged;

            // Note: Global hotkey monitoring is disabled in Avalonia (WPF-specific)
            // Hotkey functionality can be re-implemented using Avalonia-specific approaches if needed
        }

        #region Bounty Run Methods

        public void SetCurrentBountyRun(BountyRun bountyRun)
        {
            // Close the previous run before starting a new one.
            StopCurrentBountyRun();

            CurrentBountyRun = bountyRun;
            IsBountyRunActive = true;

            // Add to the beginning of the collection
            BountyRuns.Insert(0, bountyRun);

            TopBountyRuns = new ObservableCollection<BountyRun>(BountyRuns.Take(3));
        }

        public void StopCurrentBountyRun()
        {
            if (CurrentBountyRun == null || CurrentBountyRun.IsCompleted) return;

            CurrentBountyRun.EndTime = DateTime.Now;
            CurrentBountyRun.IsCompleted = true;


            // Reset current run
            //CurrentBountyRun = null;
            IsBountyRunActive = false;
        }

        public void UpdateCurrentBountyRunIsk(int iskChange, Character character)
        {
            if (CurrentBountyRun != null && !CurrentBountyRun.IsCompleted)
            {
                CurrentBountyRun.TotalIsk += iskChange;

                // Fire-and-forget API call with settings integration
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendBountyToJournalApi(iskChange, character);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't crash the UI
                        System.Diagnostics.Debug.WriteLine($"Error sending bounty to journal API: {ex.Message}");
                    }
                });
            }
        }

        private async Task SendBountyToJournalApi(int iskChange, Character character)
        {
            try
            {
                // Get API key from settings
                var apiKeySetting = await _settingRepository.GetByKeyAsync("ApiKey");
                if (string.IsNullOrEmpty(apiKeySetting?.Value))
                {
                    System.Diagnostics.Debug.WriteLine("No API key configured for journal API");
                    return;
                }

                // Check if force bounty to one user is enabled
                var forceBountySetting = await _settingRepository.GetByKeyAsync("ForceBountyToOneUser");
                var shouldForce = bool.TryParse(forceBountySetting?.Value, out var forceValue) && forceValue;

                Character targetCharacter = character;

                if (shouldForce)
                {
                    var selectedCharacterSetting = await _settingRepository.GetByKeyAsync("SelectedCharacterId");
                    if (int.TryParse(selectedCharacterSetting?.Value, out var characterId))
                    {
                        targetCharacter = Characters.FirstOrDefault(c => c.CharacterId == characterId) ?? character;
                    }
                }

                // Send to journal API - Keep original format
                var baseAddress = new Uri(_configuration["ApiSettings:BaseUrl"] ?? string.Empty);
                var eveJournalApiUrl = $"{baseAddress}/v1/eve-journal/realtime-bounty-update/{targetCharacter.CharacterId}";

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKeySetting.Value);

                // Keep original content format - just send total ISK as plain text
                var content = new StringContent(CurrentBountyRun.TotalIsk.ToString(), System.Text.Encoding.UTF8, "text/plain");
                var response = await httpClient.PostAsync(eveJournalApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully sent total ISK ({CurrentBountyRun.TotalIsk:N0}) to journal API for character {targetCharacter.Name}");
                    if (shouldForce && targetCharacter.CharacterId != character.CharacterId)
                    {
                        System.Diagnostics.Debug.WriteLine($"Bounty redirected from {character.Name} to {targetCharacter.Name}");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Failed to send to journal API: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error sending to journal API: {httpEx.Message}");
            }
            catch (TaskCanceledException tcEx)
            {
                System.Diagnostics.Debug.WriteLine($"Timeout sending to journal API: {tcEx.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error sending to journal API: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        private void ShowView(object view)
        {
            CurrentView = view;
        }

        private void WatchCharacter(Character character)
        {
            character.PropertyChanged += OnCharacterPropertyChanged;
        }

        private void OnCharacterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Toggling a character active/inactive must immediately add/remove it from the list.
            if (e.PropertyName == nameof(Character.Active))
                Avalonia.Threading.Dispatcher.UIThread.Post(FilterCharacters);
        }

        private void FilterCharacters()
        {
            IEnumerable<Character> visible = Characters.Where(c => c.Active);

            if (!ShowOfflineCharacters)
                visible = visible.Where(c => c.Online);

            var ordered = visible
                .OrderByDescending(c => c.Online)
                .ThenBy(c => c.Position)
                .ThenBy(c => c.Name);

            FilteredCharacters = new ObservableCollection<Character>(ordered);
            OnPropertyChanged(nameof(FilteredCharacters));
        }

        private void OnCharacterStatusChanged(object? sender, CharacterStatusChangedEventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var character = Characters.FirstOrDefault(c => c.CharacterId == e.CharacterId);
                if (character != null)
                {
                    // Note: No need to set character.Online here - it's already set by CheckOnlineJob
                    // Setting it again would cause duplicate PropertyChanged events
                    FilterCharacters();
                }
            });
        }

        // Note: OnHotkeyPressed is disabled in Avalonia (WPF-specific global hotkey functionality)
        // This functionality can be exposed through UI buttons or re-implemented using Avalonia-specific approaches
        /*
        private void OnHotkeyPressed(int hotkeyId, string hotkeyName)
        {
            try
            {
                switch (hotkeyName)
                {
                    case "ResetBounty":
                        StopCurrentBountyRun();

                        var runCount = BountyRuns?.Count + 1 ?? 1;
                        var currentTime = DateTime.Now.ToString("h:mm tt");
                        var runName = $"Run #{runCount}, {currentTime}";

                        var bountyRun = new BountyRun
                        {
                            Id = DateTime.Now.Ticks.GetHashCode(), // Simple ID generation for in-memory
                            Name = runName,
                            StartTime = DateTime.Now,
                            TotalIsk = 0,
                            IsCompleted = false
                        };

                        SetCurrentBountyRun(bountyRun);

                        break;
                    case "OpenJournal":
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = _configuration.GetValue<string>("EveJournalUrl"),
                            UseShellExecute = true
                        });

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling hotkey {hotkeyName}: {ex.Message}");
            }
        }
        */

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}