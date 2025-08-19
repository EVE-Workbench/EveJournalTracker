using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using EWB_Tracker.Commands;
using SharedLibrary.Cache;
using SharedLibrary.Models;
using SharedLibrary.Repositories.Interfaces;

namespace EWB_Tracker.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ISettingRepository _settingRepository;
        private readonly CharacterCache _characterCache;
        
        private string _apiKey = string.Empty;
        private bool _forceBountyToOneUser = false;
        private Character _selectedCharacter;
        private bool _isLoading = false;

        public SettingsViewModel(ISettingRepository settingRepository, CharacterCache characterCache)
        {
            _settingRepository = settingRepository;
            _characterCache = characterCache;
            
            Characters = new ObservableCollection<Character>();
            
            SaveCommand = new RelayCommand(() => _ = SaveSettings(), () => !IsLoading);
            LoadCommand = new RelayCommand(() => _ = LoadSettings(), () => !IsLoading);
            
            // Load initial data
            _ = LoadCharactersAndSettings();
        }

        #region Properties

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                OnPropertyChanged();
            }
        }

        public bool ForceBountyToOneUser
        {
            get => _forceBountyToOneUser;
            set
            {
                _forceBountyToOneUser = value;
                OnPropertyChanged();
                // If disabled, clear selected character
                if (!value)
                {
                    SelectedCharacter = null;
                }
            }
        }

        public Character SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                _selectedCharacter = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                // Force command refresh - your RelayCommand uses CommandManager
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<Character> Characters { get; }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }

        #endregion

        #region Private Methods

        private async Task LoadCharactersAndSettings()
        {
            IsLoading = true;
            try
            {
                await LoadCharacters();
                await LoadSettings();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCharacters()
        {
            var characters = _characterCache.GetAllCharacters();
            
            Characters.Clear();
            foreach (var character in characters.OrderBy(c => c.Name))
            {
                Characters.Add(character);
            }
        }

        private async Task LoadSettings()
        {
            try
            {
                // Load API Key
                var apiKeySetting = await _settingRepository.GetByKeyAsync("ApiKey");
                ApiKey = apiKeySetting?.Value ?? string.Empty;

                // Load Force Bounty setting
                var forceBountySetting = await _settingRepository.GetByKeyAsync("ForceBountyToOneUser");
                ForceBountyToOneUser = bool.TryParse(forceBountySetting?.Value, out var forceValue) && forceValue;

                // Load Selected Character
                var selectedCharacterSetting = await _settingRepository.GetByKeyAsync("SelectedCharacterId");
                if (int.TryParse(selectedCharacterSetting?.Value, out var characterId))
                {
                    SelectedCharacter = Characters.FirstOrDefault(c => c.CharacterId == characterId);
                }
            }
            catch (Exception ex)
            {
                // Log error or show message to user
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private async Task SaveSettings()
        {
            IsLoading = true;
            try
            {
                // Save API Key
                await _settingRepository.UpsertAsync("ApiKey", ApiKey ?? string.Empty);

                // Save Force Bounty setting
                await _settingRepository.UpsertAsync("ForceBountyToOneUser", ForceBountyToOneUser.ToString());

                // Save Selected Character (only if force bounty is enabled)
                if (ForceBountyToOneUser && SelectedCharacter != null)
                {
                    await _settingRepository.UpsertAsync("SelectedCharacterId", SelectedCharacter.CharacterId.ToString());
                }
                else
                {
                    await _settingRepository.UpsertAsync("SelectedCharacterId", string.Empty);
                }

                // Show success message or notification
                System.Diagnostics.Debug.WriteLine("Settings saved successfully!");
            }
            catch (Exception ex)
            {
                // Log error or show error message to user
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Public Methods

        public async Task RefreshCharacters()
        {
            await LoadCharacters();
        }

        public async Task RefreshSettings()
        {
            await LoadSettings();
        }

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