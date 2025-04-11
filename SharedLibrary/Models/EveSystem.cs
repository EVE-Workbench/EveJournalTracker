using System.ComponentModel;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Models;

public class EveSystem : INotifyPropertyChanged
{
    private int _bounty;
    private DateTime _lastUpdated;

    public EveSystemDto EveSystemDto { get; set; }

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

    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set
        {
            if (_lastUpdated != value)
            {
                _lastUpdated = value;
                OnPropertyChanged(nameof(LastUpdated));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}