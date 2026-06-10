using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using UI.avalonia.Input;

namespace UI.avalonia.Services;

/// <summary>
/// Owns the configurable shortcut bindings and turns input into command triggers.
///
/// In-app handling (keyboard + mouse) works on every platform while the window is focused. Global
/// (window-unfocused) handling is added per platform: Windows via Win32 (<see cref="GlobalHotkeyService"/>),
/// Linux via the XDG GlobalShortcuts portal (<see cref="GlobalShortcutsPortal"/>). When both the global
/// path and the in-app path fire for the same press, <see cref="Fire"/> debounces so the command runs once.
/// Mouse buttons are in-app only.
/// </summary>
public class ShortcutService
{
    private readonly GlobalHotkeyService _global;
    private GlobalShortcutsPortal? _portal;
    private readonly Dictionary<string, ShortcutGesture> _bindings;

    private readonly object _fireLock = new();
    private readonly Dictionary<string, DateTime> _lastFired = new();

    public event Action<string>? Triggered;
    public event Action? BindingsChanged;

    public ShortcutService(GlobalHotkeyService global)
    {
        _global = global;
        _bindings = ShortcutStore.Load();
        _global.HotkeyPressed += (_, commandId) => Fire(commandId);
    }

    public ShortcutGesture GetBinding(string commandId) =>
        _bindings.TryGetValue(commandId, out var gesture) ? gesture : new ShortcutGesture();

    /// <summary>Registers the keyboard bindings for global (window-unfocused) use, per platform.</summary>
    public void RegisterGlobals()
    {
        foreach (var commandId in _bindings.Keys)
            _global.UnregisterHotkey(commandId);

        foreach (var (commandId, gesture) in _bindings)
            if (!gesture.IsMouse && gesture.Key != Key.None)
                _global.RegisterHotkey(commandId, ToGlobalModifiers(gesture.Modifiers), gesture.Key);

        EnsureLinuxPortal();
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

        // Re-register the Windows global keyboard hotkeys. The Linux portal binding is owned by the
        // desktop (changed in its shortcut settings), so it is only set up once at startup.
        foreach (var id in _bindings.Keys)
            _global.UnregisterHotkey(id);
        foreach (var (id, g) in _bindings)
            if (!g.IsMouse && g.Key != Key.None)
                _global.RegisterHotkey(id, ToGlobalModifiers(g.Modifiers), g.Key);

        BindingsChanged?.Invoke();
    }

    public bool HandleKeyDown(KeyModifiers modifiers, Key key)
    {
        if (IsModifierKey(key))
            return false;

        foreach (var (commandId, gesture) in _bindings)
            if (gesture.Matches(modifiers, key))
            {
                Fire(commandId);
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
                Fire(commandId);
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

    private void Fire(string commandId)
    {
        lock (_fireLock)
        {
            var now = DateTime.UtcNow;
            if (_lastFired.TryGetValue(commandId, out var last) && (now - last).TotalMilliseconds < 250)
                return;
            _lastFired[commandId] = now;
        }

        Triggered?.Invoke(commandId);
    }

    private void EnsureLinuxPortal()
    {
        if (_portal != null || !OperatingSystem.IsLinux())
            return;

        _portal = new GlobalShortcutsPortal();
        _portal.Activated += Fire;
        _ = _portal.StartAsync(BuildPortalShortcuts());
    }

    private List<PortalShortcut> BuildPortalShortcuts()
    {
        var shortcuts = new List<PortalShortcut>();
        foreach (var command in ShortcutCommands.All)
        {
            var gesture = GetBinding(command.Id);
            shortcuts.Add(new PortalShortcut(command.Id, command.DisplayName, KeyboardTriggerString(gesture)));
        }
        return shortcuts;
    }

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

    /// <summary>
    /// Best-effort XDG portal trigger string (e.g. "CTRL+SHIFT+n") for a keyboard gesture, or null
    /// for mouse/unsupported keys so the user binds it in their desktop's shortcut settings.
    /// </summary>
    private static string? KeyboardTriggerString(ShortcutGesture gesture)
    {
        if (gesture.IsMouse || gesture.Key == Key.None)
            return null;

        var key = KeysymName(gesture.Key);
        if (key == null)
            return null;

        var parts = new List<string>();
        if (gesture.Modifiers.HasFlag(KeyModifiers.Control)) parts.Add("CTRL");
        if (gesture.Modifiers.HasFlag(KeyModifiers.Shift)) parts.Add("SHIFT");
        if (gesture.Modifiers.HasFlag(KeyModifiers.Alt)) parts.Add("ALT");
        if (gesture.Modifiers.HasFlag(KeyModifiers.Meta)) parts.Add("LOGO");
        parts.Add(key);
        return string.Join("+", parts);
    }

    private static string? KeysymName(Key key)
    {
        if (key is >= Key.A and <= Key.Z)
            return key.ToString().ToLowerInvariant();
        if (key is >= Key.D0 and <= Key.D9)
            return ((int)(key - Key.D0)).ToString();
        if (key is >= Key.F1 and <= Key.F12)
            return key.ToString();
        return null;
    }
}
