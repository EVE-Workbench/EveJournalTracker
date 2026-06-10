using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.avalonia.ViewModels;

public partial class ShortcutRowViewModel : ViewModelBase
{
    public string CommandId { get; }
    public string DisplayName { get; }

    [ObservableProperty] private string _gestureDisplay;
    [ObservableProperty] private bool _isRecording;

    public ShortcutRowViewModel(string commandId, string displayName, string gestureDisplay)
    {
        CommandId = commandId;
        DisplayName = displayName;
        _gestureDisplay = gestureDisplay;
    }
}
