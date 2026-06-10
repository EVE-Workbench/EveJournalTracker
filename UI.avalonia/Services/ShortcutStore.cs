using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Input;
using SharedLibrary.Utils;
using UI.avalonia.Input;

namespace UI.avalonia.Services;

/// <summary>
/// Persists the user's shortcut bindings to JSON in the per-user data folder, merged over the
/// built-in defaults so newly added commands always have a sensible binding.
/// </summary>
public static class ShortcutStore
{
    private static readonly string FilePath = Path.Combine(AppPaths.DataDirectory, "shortcuts.json");

    public static Dictionary<string, ShortcutGesture> Load()
    {
        var bindings = Defaults();
        try
        {
            if (File.Exists(FilePath))
            {
                var saved = JsonSerializer.Deserialize<Dictionary<string, ShortcutGesture>>(File.ReadAllText(FilePath));
                if (saved != null)
                    foreach (var kvp in saved)
                        bindings[kvp.Key] = kvp.Value;
            }
        }
        catch
        {
            // Corrupt file: fall back to defaults.
        }

        return bindings;
    }

    public static void Save(Dictionary<string, ShortcutGesture> bindings)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.DataDirectory);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(bindings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Best-effort persistence.
        }
    }

    private static Dictionary<string, ShortcutGesture> Defaults() => new()
    {
        [ShortcutCommands.NewBountyRun] = new ShortcutGesture
        {
            Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
            Key = Key.N
        },
        [ShortcutCommands.OpenEveJournal] = new ShortcutGesture
        {
            Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
            Key = Key.J
        }
    };
}
