using System.IO;

namespace SharedLibrary.Utils;

public class EveUtils
{
    public static string GetDefaultLogFolderLocation()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "EVE",
            "logs",
            "Gamelogs"
        );
    }
}