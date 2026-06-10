namespace SharedLibrary.Utils;

/// <summary>
/// Per-user locations for application data. Keeping the database and settings outside the
/// install folder means an update (which replaces the install folder) can never overwrite
/// them, and they survive reinstalls.
/// </summary>
public static class AppPaths
{
    private const string FolderName = "EveJournalTracker";
    private const string DatabaseFileName = "eve_tracker.db";

    /// <summary>
    /// %APPDATA%\EveJournalTracker on Windows, ~/.config/EveJournalTracker on Linux/macOS.
    /// </summary>
    public static string DataDirectory
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                FolderName);
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string DatabasePath => Path.Combine(DataDirectory, DatabaseFileName);

    /// <summary>
    /// Older versions stored the database next to the executable. When a legacy file exists and
    /// the new per-user copy does not, copy it over (with its WAL/SHM siblings) so existing data
    /// carries across the move. The legacy file is left untouched.
    /// </summary>
    public static void MigrateLegacyDatabase()
    {
        if (File.Exists(DatabasePath))
            return;

        foreach (var baseDir in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var legacy = Path.Combine(baseDir, DatabaseFileName);
            if (!File.Exists(legacy))
                continue;

            foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
            {
                var source = legacy + suffix;
                if (File.Exists(source))
                    File.Copy(source, DatabasePath + suffix, overwrite: false);
            }
            return;
        }
    }
}
