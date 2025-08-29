using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SharedLibrary.Models;

public class BountyRun : INotifyPropertyChanged
{
    private int _totalIsk;
    private bool _isCompleted;
    private DateTime? _endTime;

    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    
    public int TotalIsk 
    { 
        get => _totalIsk;
        set 
        {
            _totalIsk = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsCompleted 
    { 
        get => _isCompleted;
        set 
        {
            _isCompleted = value;
            OnPropertyChanged();
        }
    }
    
    public DateTime? EndTime 
    { 
        get => _endTime;
        set 
        {
            _endTime = value;
            OnPropertyChanged();
        }
    }
    
    public DateTime StartTime { get; set; }
    public int? CharacterId { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}