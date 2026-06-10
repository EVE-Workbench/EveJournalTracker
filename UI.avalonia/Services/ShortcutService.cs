using System;
using System.Collections.Generic;
using Avalonia.Input;
using UI.avalonia.Input;

namespace UI.avalonia.Services;

/// <summary>
/// Owns the configurable shortcut bindings and turns input into command triggers.
///
/// In-app handling (keyboard + mouse) works on every platform while the window is focused.
/// On Windows, keyboard shortcuts are additionally registered system-wide via
/// <see cref="GlobalHotkeyService"/> so they fire while EVE is in the foreground; to avoid
/// double-firing, in-app keyboard matching is skipped when the global path is active. Mouse
/// buttons are in-app only (a system-wide mouse hook is unreliable and unavailable on Wayland).
/// </summary>
public class ShortcutService
{
    private readonly GlobalHotkeyService _global;
    private Dictionary<string, ShortcutGesture> _bindings;

    public event Action<string>? Triggered;
    public event Action? BindingsChanged;

    public ShortcutService(GlobalHotkeyService global)
    {
        _global = global;
        _bindings = ShortcutStore.Load();
        _global.HotkeyPressed += (_, commandId) => Triggered?.Invoke(commandId);
    }

    public bool GlobalKeyboardActive => _global.IsActive;

    public ShortcutGesture GetBinding(string commandId) =>
        _bindings.TryGetValue(commandId, out var gesture) ? gesture : new ShortcutGesture();

    /// <summary>Registers the keyboard bindings system-wide (Windows only); no-op elsewhere.</summary>
    public void RegisterGlobals()
    {
        foreach (var commandId in _bindings.Keys)
            _global.UnregisterHotkey(commandId);

        foreach (var (commandId, gesture) in _bindings)
            if (!gesture.IsMouse && gesture.Key != Key.None)
                _global.RegisterHotkey(commandId, ToGlobalModifiers(gesture.Modifiers), gesture.Key);
    }

    public void Update(string commandId, ShortcutGesture gesture)
    {
        // Keep bindings unique: clear any other command that used the same gesture.
        if (!gesture.IsEmpty)
            foreach (var (otherId, other) in _bindings)
                if (otherId != commandId && other.SameAs(gesture))
                    _bindings[otherId] = new ShortcutGesture();

        _bindings[commandId] = gesture;
        ShortcutStore.Save(_bindings);
        RegisterGlobals();
        BindingsChanged?.Invoke();
    }

    public bool HandleKeyDown(KeyModifiers modifiers, Key key)
    {
        // On Windows the global hotkey already fires (even when focused), so skip the in-app path.
        if (GlobalKeyboardActive || IsModifierKey(key))
            return false;

        foreach (var (commandId, gesture) in _bindings)
            if (gesture.Matches(modifiers, key))
            {
                Triggered?.Invoke(commandId);
                return true;
            }

        return false;
    }

    public bool HandlePointer(KeyModifiers modifiers, ShortcutMouseButton button)
    {
        if (button == ShortcutMouseButton.None)
            return false;

        foreach (var (commandId, gesture) in _bindings)
            if (gesture.Matches(modifiers, button))
            {
                Triggered?.Invoke(commandId);
                return true;
            }

        return false;
    }

    public static ShortcutMouseButton ToMouseButton(PointerUpdateKind kind) => kind switch
    {
        PointerUpdateKind.MiddleButtonPressed => ShortcutMouseButton.Middle,
        PointerUpdateKind.XButton1Pressed => ShortcutMouseButton.XButton1,
        PointerUpdateKind.XButton2Pressed => ShortcutMouseButton.XButton2,
        _ => ShortcutMouseButton.None
    };

    private static bool IsModifierKey(Key key) => key is
        Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or
        Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin;

    private static GlobalHotkeyService.ModifierKeys ToGlobalModifiers(KeyModifiers modifiers)
    {
        var result = GlobalHotkeyService.ModifierKeys.None;
        if (modifiers.HasFlag(KeyModifiers.Control)) result |= GlobalHotkeyService.ModifierKeys.Control;
        if (modifiers.HasFlag(KeyModifiers.Shift)) result |= GlobalHotkeyService.ModifierKeys.Shift;
        if (modifiers.HasFlag(KeyModifiers.Alt)) result |= GlobalHotkeyService.ModifierKeys.Alt;
        if (modifiers.HasFlag(KeyModifiers.Meta)) result |= GlobalHotkeyService.ModifierKeys.Win;
        return result;
    }
}
