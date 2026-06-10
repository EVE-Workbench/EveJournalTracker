namespace UI.avalonia.Input;

/// <summary>
/// Mouse buttons that can be bound to a shortcut. Left/right are excluded so normal interaction
/// keeps working; middle and the two extra side buttons (mouse 4/5) are available.
/// </summary>
public enum ShortcutMouseButton
{
    None,
    Middle,
    XButton1,
    XButton2
}
