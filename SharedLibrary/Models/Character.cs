using System.ComponentModel;

namespace SharedLibrary.Models
{
    public class Character : INotifyPropertyChanged
    {
        private string _name;
        private bool _online;
        private int _bounty;
        private EveSystem? _eveSystem;

        public int CharacterId { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}