using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SharedLibrary.Services;


public class ClipboardMonitorService : IDisposable
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;
    private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

    private HwndSource _hwndSource;
    private bool _isListening;

    public event Action<string> ClipboardChanged;

    public void StartMonitoring(Window window)
    {
        if (_isListening) return;

        var helper = new WindowInteropHelper(window);
        var hwnd = helper.Handle;

        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource.AddHook(WndProc);

        if (AddClipboardFormatListener(hwnd))
        {
            _isListening = true;
            Console.WriteLine("Clipboard monitoring started");
        }
        else
        {
            Console.WriteLine("Failed to start clipboard monitoring");
        }
    }

    public void StopMonitoring()
    {
        if (!_isListening) return;

        if (_hwndSource?.Handle != IntPtr.Zero)
        {
            RemoveClipboardFormatListener(_hwndSource.Handle);
            _hwndSource.RemoveHook(WndProc);
        }

        _isListening = false;
        Console.WriteLine("Clipboard monitoring stopped");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            OnClipboardChanged();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void OnClipboardChanged()
    {
        try
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                
                // Ignore empty strings
                if (!string.IsNullOrWhiteSpace(clipboardText))
                {
                    Console.WriteLine($"Clipboard changed: {clipboardText.Substring(0, Math.Min(50, clipboardText.Length))}...");
                    ClipboardChanged?.Invoke(clipboardText);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading clipboard: {ex.Message}");
        }
    }

    public void Dispose()
    {
        StopMonitoring();
        _hwndSource?.Dispose();
    }

    #region Windows API
    
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    #endregion
}