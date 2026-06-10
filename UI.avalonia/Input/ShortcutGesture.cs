using System.Collections.Generic;
using Avalonia.Input;

namespace UI.avalonia.Input;

/// <summary>
/// A bindable shortcut: a set of modifiers plus either a keyboard key or a mouse button.
/// Serialized to JSON for persistence (enums store as numbers).
/// </summary>
public sealed class ShortcutGesture
{
    public KeyModifiers Modifiers { get; set; } = KeyModifiers.None;
    public Key Key { get; set; } = Key.None;
    public ShortcutMouseButton MouseButton { get; set; } = ShortcutMouseButton.None;

    public bool IsEmpty => Key == Key.None && MouseButton == ShortcutMouseButton.None;
    public bool IsMouse => MouseButton != ShortcutMouseButton.None;

    public bool Matches(KeyModifiers modifiers, Key key) =>
        !IsMouse && Key != Key.None && key == Key && modifiers == Modifiers;

    public bool Matches(KeyModifiers modifiers, ShortcutMouseButton button) =>
        IsMouse && button != ShortcutMouseButton.None && button == MouseButton && modifiers == Modifiers;

    public bool SameAs(ShortcutGesture other) =>
        !IsEmpty && Modifiers == other.Modifiers && Key == other.Key && MouseButton == other.MouseButton;

    public string ToDisplayString()
    {
        if (IsEmpty)
            return "—";

        var parts = new List<string>();
        if (Modifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(KeyModifiers.Meta)) parts.Add("Win");
        parts.Add(IsMouse ? MouseName(MouseButton) : Key.ToString());
        return string.Join(" + ", parts);
    }

    private static string MouseName(ShortcutMouseButton button) => button switch
    {
        ShortcutMouseButton.Middle => "Mouse3",
        ShortcutMouseButton.XButton1 => "Mouse4",
        ShortcutMouseButton.XButton2 => "Mouse5",
        _ => "Mouse"
    };
}
