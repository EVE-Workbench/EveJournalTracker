using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;

namespace UI.avalonia.Services;

public class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int WmDestroy = 0x0002;
    private readonly Dictionary<int, HotkeyInfo> _hotkeys = new Dictionary<int, HotkeyInfo>();
    private int _currentId = 9000;
    private IntPtr _windowHandle = IntPtr.Zero;
    private bool _disposed = false;
    private bool _isWindows = false;
    private IntPtr _messageWindowHandle = IntPtr.Zero;

    public delegate void HotkeyPressedEventHandler(int hotkeyId, string hotkeyName);
    public event HotkeyPressedEventHandler? HotkeyPressed;

    /// <summary>True when system-wide keyboard hotkeys are available (Windows, initialized).</summary>
    public bool IsActive => _isWindows && _messageWindowHandle != IntPtr.Zero;

    // Windows API imports
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
        IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    // Modifier key flags (Windows API)
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

    private WndProc _wndProcDelegate;

    public void Initialize(Window window)
    {
        if (_messageWindowHandle != IntPtr.Zero)
            return;

        // Check if we're on Windows
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (_isWindows)
        {
            try
            {
                // Store main window handle for reference
                _windowHandle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;

                // Create a message-only window for hotkey messages
                CreateMessageWindow();

                if (_messageWindowHandle != IntPtr.Zero)
                {
                    // Start message loop
                    StartMessageLoop();

                    // Clean up when main window closes
                    window.Closed += (s, e) => Dispose();
                }
            }
            catch (Exception)
            {
                _isWindows = false;
            }
        }
    }

    private void CreateMessageWindow()
    {
        // Keep delegate alive to prevent garbage collection
        _wndProcDelegate = MessageWindowProc;

        var hInstance = GetModuleHandle(null);
        var className = "GlobalHotkeyMessageWindow_" + Guid.NewGuid().ToString();

        var wc = new WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = hInstance,
            lpszClassName = className
        };

        var classAtom = RegisterClassEx(ref wc);
        if (classAtom == 0)
            return;

        // HWND_MESSAGE (-3) creates a message-only window
        _messageWindowHandle = CreateWindowEx(
            0, className, "GlobalHotkeyWindow", 0,
            0, 0, 0, 0, new IntPtr(-3), IntPtr.Zero, hInstance, IntPtr.Zero);
    }

    private IntPtr MessageWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WmHotkey)
        {
            var hotkeyId = wParam.ToInt32();
            ProcessHotkeyMessage(hotkeyId);
            return IntPtr.Zero;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void StartMessageLoop()
    {
        // Create a dedicated background thread for message loop
        var messageThread = new System.Threading.Thread(() =>
        {
            while (!_disposed && _messageWindowHandle != IntPtr.Zero)
            {
                try
                {
                    // Use GetMessage - it will block until a message arrives
                    var result = GetMessage(out var msg, _messageWindowHandle, 0, 0);

                    if (result == 0 || result == -1)
                        break;

                    // Translate and dispatch the message
                    // The WndProc will handle WM_HOTKEY
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
                catch
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        })
        {
            IsBackground = true,
            Name = "GlobalHotkeyMessageLoop"
        };

        messageThread.Start();
    }

    public bool RegisterHotkey(string name, ModifierKeys modifiers, Key key)
    {
        if (!_isWindows || _messageWindowHandle == IntPtr.Zero)
            return false;

        var id = _currentId++;
        var vkCode = GetVirtualKeyCode(key);

        if (vkCode == 0)
            return false;

        var success = RegisterHotKey(_messageWindowHandle, id, (uint)modifiers, vkCode);

        if (success)
        {
            _hotkeys[id] = new HotkeyInfo
            {
                Name = name,
                Modifiers = modifiers,
                Key = key
            };
        }

        return success;
    }

    public bool UnregisterHotkey(string name)
    {
        if (!_isWindows)
            return false;

        var hotkeyToRemove = -1;
        foreach (var kvp in _hotkeys)
        {
            if (kvp.Value.Name == name)
            {
                hotkeyToRemove = kvp.Key;
                break;
            }
        }

        if (hotkeyToRemove == -1)
            return false;

        var success = UnregisterHotKey(_messageWindowHandle, hotkeyToRemove);
        if (success)
            _hotkeys.Remove(hotkeyToRemove);

        return success;
    }

    // Convert Avalonia Key to Windows Virtual Key Code
    private uint GetVirtualKeyCode(Key key)
    {
        return key switch
        {
            Key.N => 0x4E, // N
            Key.J => 0x4A, // J
            Key.A => 0x41,
            Key.B => 0x42,
            Key.C => 0x43,
            Key.D => 0x44,
            Key.E => 0x45,
            Key.F => 0x46,
            Key.G => 0x47,
            Key.H => 0x48,
            Key.I => 0x49,
            Key.K => 0x4B,
            Key.L => 0x4C,
            Key.M => 0x4D,
            Key.O => 0x4F,
            Key.P => 0x50,
            Key.Q => 0x51,
            Key.R => 0x52,
            Key.S => 0x53,
            Key.T => 0x54,
            Key.U => 0x55,
            Key.V => 0x56,
            Key.W => 0x57,
            Key.X => 0x58,
            Key.Y => 0x59,
            Key.Z => 0x5A,
            Key.D0 => 0x30,
            Key.D1 => 0x31,
            Key.D2 => 0x32,
            Key.D3 => 0x33,
            Key.D4 => 0x34,
            Key.D5 => 0x35,
            Key.D6 => 0x36,
            Key.D7 => 0x37,
            Key.D8 => 0x38,
            Key.D9 => 0x39,
            Key.F1 => 0x70,
            Key.F2 => 0x71,
            Key.F3 => 0x72,
            Key.F4 => 0x73,
            Key.F5 => 0x74,
            Key.F6 => 0x75,
            Key.F7 => 0x76,
            Key.F8 => 0x77,
            Key.F9 => 0x78,
            Key.F10 => 0x79,
            Key.F11 => 0x7A,
            Key.F12 => 0x7B,
            _ => 0
        };
    }

    public void ProcessHotkeyMessage(int hotkeyId)
    {
        if (_hotkeys.TryGetValue(hotkeyId, out var hotkeyInfo))
            HotkeyPressed?.Invoke(hotkeyId, hotkeyInfo.Name);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_isWindows && _messageWindowHandle != IntPtr.Zero)
        {
            // Unregister all hotkeys
            foreach (var id in _hotkeys.Keys)
                UnregisterHotKey(_messageWindowHandle, id);

            // Destroy the message window
            DestroyWindow(_messageWindowHandle);
            _messageWindowHandle = IntPtr.Zero;
        }

        _hotkeys.Clear();
        _windowHandle = IntPtr.Zero;
    }
}
