using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace SharedLibrary.Services;

public class GlobalHotkeyHandlerService
{
    public void Initialize(GlobalHotkeyMonitorService globalHotkeyMonitorService)
    {
        globalHotkeyMonitorService.RegisterHotkey("ResetBounty", GlobalHotkeyMonitorService.ModifierKeys.Control | GlobalHotkeyMonitorService.ModifierKeys.Shift, Key.N);
        
        // Subscribe to the HotkeyPressed event
        //globalHotkeyMonitorService.HotkeyPressed += OnHotkeyPressed;
    }
    
    // Event handler for hotkey presses
    /*private void OnHotkeyPressed(int hotkeyId, string hotkeyName)
    {
        try
        {
            switch (hotkeyName)
            {
                case "ResetBounty":
                    break;
                default:
                    Console.WriteLine($"Unhandled hotkey: {hotkeyName}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling hotkey {hotkeyName}: {ex.Message}");
        }
    }*/
}