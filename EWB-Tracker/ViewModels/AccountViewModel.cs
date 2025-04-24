using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using EWB_Tracker.Commands;
using SharedLibrary.Cache;
using SharedLibrary.Models;

namespace EWB_Tracker.ViewModels
{
    public class AccountViewModel : INotifyPropertyChanged
    {
        private readonly CharacterCache _characterCache;
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

        public ICommand SaveChangesCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        public AccountViewModel(CharacterCache characterCache)
        {
            _characterCache = characterCache ?? throw new ArgumentNullException(nameof(characterCache));
            
            // Load and sort characters by position
            var characters = _characterCache.GetAllCharacters()
                .OrderBy(c => c.Position)
                .ToList();
                
            _characters = new ObservableCollection<Character>(characters);
            
            SaveChangesCommand = new RelayCommand(SaveChanges);
            MoveUpCommand = new RelayCommand<Character>(MoveCharacterUp, CanMoveCharacterUp);
            MoveDownCommand = new RelayCommand<Character>(MoveCharacterDown, CanMoveCharacterDown);
        }

        private void SaveChanges(object parameter)
        {
            // Update positions before saving
            for (int i = 0; i < Characters.Count; i++)
            {
                Characters[i].Position = i;
            }
            
            _characterCache.SaveChanges();
        }
        
        private bool CanMoveCharacterUp(Character character)
        {
            if (character == null) return false;
            int index = Characters.IndexOf(character);
            return index > 0;
        }
        
        private void MoveCharacterUp(Character character)
        {
            if (character == null) return;
            
            int index = Characters.IndexOf(character);
            if (index > 0)
            {
                Characters.Move(index, index - 1);
                // Update positions after move
                Characters[index].Position = index;
                Characters[index - 1].Position = index - 1;
            }
        }
        
        private bool CanMoveCharacterDown(Character character)
        {
            if (character == null) return false;
            int index = Characters.IndexOf(character);
            return index < Characters.Count - 1;
        }
        
        private void MoveCharacterDown(Character character)
        {
            if (character == null) return;
            
            int index = Characters.IndexOf(character);
            if (index < Characters.Count - 1)
            {
                Characters.Move(index, index + 1);
                // Update positions after move
                Characters[index].Position = index;
                Characters[index + 1].Position = index + 1;
            }
        }
        
        public void OnCharacterDrop(Character draggedItem, Character targetItem)
        {
            if (draggedItem == null || targetItem == null || draggedItem == targetItem)
                return;
                
            int oldIndex = Characters.IndexOf(draggedItem);
            int newIndex = Characters.IndexOf(targetItem);
            
            Characters.Move(oldIndex, newIndex);
            
            // Update all positions
            for (int i = 0; i < Characters.Count; i++)
            {
                Characters[i].Position = i;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}