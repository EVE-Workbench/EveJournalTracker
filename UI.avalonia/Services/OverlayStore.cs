using System;
using System.IO;
using System.Text.Json;

namespace UI.avalonia.Services;

/// <summary>
/// Persists the DPS overlay window's position, size and opacity to a single JSON file
/// in the per-user app-data folder so the overlay reopens where the user left it.
/// </summary>
public static class OverlayStore
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EveJournalTracker");

    private static readonly string FilePath = Path.Combine(DirectoryPath, "dps-overlay.json");

    public static OverlayGeometry Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<OverlayGeometry>(File.ReadAllText(FilePath)) ?? new OverlayGeometry();
        }
        catch
        {
            // Corrupt or unreadable file: fall back to defaults.
        }

        return new OverlayGeometry();
    }

    public static void Save(OverlayGeometry geometry)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(geometry));
        }
        catch
        {
            // Best-effort persistence; ignore write failures.
        }
    }
}
