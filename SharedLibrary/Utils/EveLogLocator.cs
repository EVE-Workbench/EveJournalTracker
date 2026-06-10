using System.Text.RegularExpressions;

namespace SharedLibrary.Utils;

/// <summary>
/// Best-effort, cross-platform detection of the EVE gamelog folder. On Windows/macOS the
/// client writes to the user's Documents folder; on Linux it runs through Steam (Proton)
/// or another Wine prefix, so the logs live inside that prefix's virtual drive. This scans
/// the likely locations and returns the one that actually holds logs, falling back to the
/// most probable guess for the current OS.
/// </summary>
public static class EveLogLocator
{
    // EVE Online's Steam application id; its Proton prefix lives under compatdata/8500.
    private const string EveSteamAppId = "8500";

    public static string Detect()
    {
        var candidates = Candidates().Distinct().ToList();

        var existing = candidates
            .Where(Directory.Exists)
            .OrderByDescending(LatestWrite)
            .FirstOrDefault();

        // Nothing on disk yet: hand back the most likely path for this OS so the user has
        // a sensible starting point to adjust rather than a path that cannot exist here.
        return existing ?? candidates.FirstOrDefault() ?? NativeDocumentsGuess();
    }

    private static IEnumerable<string> Candidates()
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            yield return NativeDocumentsGuess();
            yield break;
        }

        foreach (var path in LinuxCandidates())
            yield return path;

        // Last resort: a native-style path (e.g. an unusual native Linux build).
        yield return NativeDocumentsGuess();
    }

    private static IEnumerable<string> LinuxCandidates()
    {
        foreach (var root in SteamLibraryRoots())
        {
            var users = Path.Combine(root, "steamapps", "compatdata", EveSteamAppId,
                "pfx", "drive_c", "users");

            foreach (var path in PrefixGamelogPaths(users))
                yield return path;
        }

        foreach (var prefix in WinePrefixes())
        {
            var users = Path.Combine(prefix, "drive_c", "users");
            foreach (var path in PrefixGamelogPaths(users))
                yield return path;
        }
    }

    private static IEnumerable<string> PrefixGamelogPaths(string usersDir)
    {
        foreach (var user in WineUsers())
        foreach (var documents in new[] { "Documents", "My Documents" })
            yield return Path.Combine(usersDir, user, documents, "EVE", "logs", "Gamelogs");
    }

    private static IEnumerable<string> SteamLibraryRoots()
    {
        var home = Home();
        var roots = new List<string>
        {
            Path.Combine(home, ".steam", "steam"),
            Path.Combine(home, ".steam", "root"),
            Path.Combine(home, ".local", "share", "Steam"),
            Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"),
        };

        // Steam can spread games over extra library folders declared in this manifest.
        roots.AddRange(roots
            .Select(r => Path.Combine(r, "steamapps", "libraryfolders.vdf"))
            .SelectMany(ParseLibraryFolders)
            .ToList());

        return roots.Distinct();
    }

    private static IEnumerable<string> ParseLibraryFolders(string vdfPath)
    {
        if (!File.Exists(vdfPath))
            return [];

        try
        {
            var text = File.ReadAllText(vdfPath);
            return Regex.Matches(text, "\"path\"\\s*\"([^\"]+)\"")
                .Select(m => m.Groups[1].Value.Replace("\\\\", "/"))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<string> WinePrefixes()
    {
        var winePrefix = Environment.GetEnvironmentVariable("WINEPREFIX");
        if (!string.IsNullOrEmpty(winePrefix))
            yield return winePrefix;

        yield return Path.Combine(Home(), ".wine");
    }

    private static IEnumerable<string> WineUsers()
    {
        yield return "steamuser";

        var user = Environment.UserName;
        if (!string.IsNullOrEmpty(user) && user != "steamuser")
            yield return user;
    }

    private static DateTime LatestWrite(string dir)
    {
        try
        {
            var files = Directory.GetFiles(dir);
            return files.Length == 0
                ? Directory.GetLastWriteTimeUtc(dir)
                : files.Max(File.GetLastWriteTimeUtc);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private static string NativeDocumentsGuess()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrEmpty(documents))
            documents = Path.Combine(Home(), "Documents");

        return Path.Combine(documents, "EVE", "logs", "Gamelogs");
    }

    private static string Home()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home))
            home = Environment.GetEnvironmentVariable("HOME") ?? string.Empty;

        return home;
    }
}
