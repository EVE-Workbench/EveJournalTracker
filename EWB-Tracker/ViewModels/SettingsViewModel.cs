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

namespace EWB_Tracker.ViewModels;
public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ISettingRepository _settingRepository;
    private readonly CharacterCache _characterCache;
    
    private string _apiKey = string.Empty;
    private bool _forceBountyToOneUser = false;
    private Character _selectedCharacter;
    private bool _isLoading = false;
    
    public ObservableCollection<Character> Characters { get; }
    
    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }

    #endregion


    public SettingsViewModel(ISettingRepository settingRepository, CharacterCache characterCache)
    {
        SaveCommand = new RelayCommand(() => _ = SaveSettings(), () => !IsLoading);
        LoadCommand = new RelayCommand(() => _ = LoadSettings(), () => !IsLoading);

        _settingRepository = settingRepository;
        _characterCache = characterCache;
        
        Characters = [];
        
        _characterCache.CharacterAdded += OnCharacterAddedAsync;
        
    }
    
    public async Task InitializeAsync() {
        
        // Load initial data
        await LoadCharactersAndSettings();
    }
    
    private async void OnCharacterAddedAsync(object sender, Character e)
    {
        try
        {
            // Refresh characters when a new character is added
            await LoadCharacters();

            if (Characters != null && Characters.All(c => c.CharacterId != e.CharacterId))
            {
                Characters.Add(e);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating characters: {ex.Message}");
        }
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
            CommandManager.InvalidateRequerySuggested();
        }
    }

    #endregion

    

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
            ApiKey = apiKeySetting?.Value;

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

            System.Diagnostics.Debug.WriteLine("Settings saved successfully!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }


    public async Task RefreshCharacters()
    {
        IsLoading = true;
        try 
        {
            await LoadCharacters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing characters: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
        
    }

    public async Task RefreshSettings()
    {
        await LoadSettings();
    }


    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
