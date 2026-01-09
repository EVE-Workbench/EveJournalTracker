using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SharedLibrary.Services;

public class GlobalHotkeyMonitorService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private readonly Dictionary<int, HotkeyInfo> _hotkeys = new Dictionary<int, HotkeyInfo>();
    private int _currentId = 9000; // Start with a high ID to avoid conflicts
    private IntPtr _windowHandle;
    private HwndSource? _source;
    private bool _disposed = false;

    public delegate void HotkeyPressedEventHandler(int hotkeyId, string hotkeyName);
    public event HotkeyPressedEventHandler? HotkeyPressed;

    // Windows API imports
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // Modifier key flags
    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

    private struct HotkeyInfo
    {
        public string Name { get; init; }
        public ModifierKeys Modifiers { get; init; }
        public Key Key { get; init; }
    }

    public void Initialize(Window window)
    {
        if (_windowHandle != IntPtr.Zero)
            return;

        _windowHandle = new WindowInteropHelper(window).Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(HwndHook);
    }

    public bool RegisterHotkey(string name, ModifierKeys modifiers, Key key)
    {
        if (_windowHandle == IntPtr.Zero)
            throw new InvalidOperationException("Service must be initialized first");

        var id = _currentId++;
        var vkCode = KeyInterop.VirtualKeyFromKey(key);

        var success = RegisterHotKey(_windowHandle, id, (uint)modifiers, (uint)vkCode);
        
        if (success)
        {
            _hotkeys[id] = new HotkeyInfo
            {
                Name = name,
                Modifiers = modifiers,
                Key = key
            };
            
            Console.WriteLine($"Successfully registered hotkey: {name} (ID: {id})");
            return true;
        }

        Console.WriteLine($"Failed to register hotkey: {name}");
        return false;
    }

    public bool UnregisterHotkey(string name)
    {
        var hotkeyToRemove = -1;
        foreach (var kvp in _hotkeys.Where(kvp => kvp.Value.Name == name))
        {
            hotkeyToRemove = kvp.Key;
            break;
        }

        if (hotkeyToRemove == -1) return false;
        
        var success = UnregisterHotKey(_windowHandle, hotkeyToRemove);
        if (!success) return success;
            
        _hotkeys.Remove(hotkeyToRemove);
        Console.WriteLine($"Successfully unregistered hotkey: {name}");
        return success;

    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey)
        {
            var id = wParam.ToInt32();
            if (_hotkeys.TryGetValue(id, out var hotkeyInfo))
            {
                Console.WriteLine($"Hotkey pressed: {hotkeyInfo.Name}");
                HotkeyPressed?.Invoke(id, hotkeyInfo.Name);
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        // Unregister all hotkeys
        foreach (var id in _hotkeys.Keys)
        {
            UnregisterHotKey(_windowHandle, id);
        }
        _hotkeys.Clear();

        // Remove the hook
        _source?.RemoveHook(HwndHook);
        _source = null;
        _windowHandle = IntPtr.Zero;

        _disposed = true;
        Console.WriteLine("GlobalHotkeyService disposed");
    }

    // Get all registered hotkeys 
    public Dictionary<string, string> GetRegisteredHotkeys()
    {
        var result = new Dictionary<string, string>();
        foreach (var kvp in _hotkeys)
        {
            var info = kvp.Value;
            var modifierString = info.Modifiers.ToString().Replace(", ", "+");
            result[info.Name] = $"{modifierString}+{info.Key}";
        }
        return result;
    }
}
