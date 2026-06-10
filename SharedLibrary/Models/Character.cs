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
        private EveSystemDto? _eveSystem;
        private bool _active = true;
        private int _position;

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

        public EveSystemDto? EveSystem
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
        
        public bool Active
        {
            get => _active;
            set
            {
                if (_active != value)
                {
                    _active = value;
                    OnPropertyChanged(nameof(Active));
                }
            }
        }
        
        public int Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
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
                Active = dto.Active,
                Position = dto.Position,
                EveSystem = null, // TODO: Link to eveSystem from memory
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
            dto.Active = Active;
            dto.Position = Position;
            
            return dto;
        }
    }
}