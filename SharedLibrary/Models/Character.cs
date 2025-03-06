using System.ComponentModel;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Models
{
    public class Character : INotifyPropertyChanged
    {
        public int CharacterId { get; set; }
        
        private string _name;
        private bool _online;
        private int _bounty;
        private EveSystem? _eveSystem;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        
        public bool Online
        {
            get => _online;
            set
            {
                if (_online != value)
                {
                    _online = value;
                    OnPropertyChanged(nameof(Online));
                }
            }
        }

        public int Bounty
        {
            get => _bounty;
            set
            {
                if (_bounty != value)
                {
                    _bounty = value;
                    OnPropertyChanged(nameof(Bounty));
                }
            }
        }

        public EveSystem? EveSystem
        {
            get => _eveSystem;
            set
            {
                if (_eveSystem != value)
                {
                    _eveSystem = value;
                    OnPropertyChanged(nameof(EveSystem));
                }
            }
        }
        
        public ICollection<LogEvent> LogEvents { get; set; } = new List<LogEvent>();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public static Character FromDto(CharacterDto dto)
        {
            return new Character
            {
                CharacterId = dto.CharacterId,
                Name = dto.Name,
                Online = dto.Online,
                Bounty = dto.Bounty,
                EveSystem = null, // TODO: Link to eveSystem from memory (on startup, load all eve systems from the database into memory and link the eve system to the character in memory)
            };
        }
        
        public CharacterDto ToDto(CharacterDto? dto = null)
        {
            dto ??= new CharacterDto
            {
                CharacterId = CharacterId,
                Name = Name
            };
            
            dto.Name = Name;
            dto.Online = Online;
            dto.Bounty = Bounty;
            dto.EveSystemId = EveSystem?.SystemId;
            
            return dto;
        }
            
    }
}