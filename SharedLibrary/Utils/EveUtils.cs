namespace SharedLibrary.Utils;

public class EveUtils
{
    public static string GetDefaultLogFolderLocation() => EveLogLocator.Detect();
}