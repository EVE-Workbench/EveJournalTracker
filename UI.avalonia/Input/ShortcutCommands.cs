using System.Collections.Generic;

namespace UI.avalonia.Input;

public sealed record ShortcutCommand(string Id, string DisplayName);

/// <summary>
/// The actions a user can bind a shortcut to. Add an entry here to make a new action bindable;
/// it will show up in Settings automatically.
/// </summary>
public static class ShortcutCommands
{
    public const string NewBountyRun = "NewBountyRun";
    public const string OpenEveJournal = "OpenEveJournal";

    public static IReadOnlyList<ShortcutCommand> All { get; } =
    [
        new(NewBountyRun, "Start new bounty run"),
        new(OpenEveJournal, "Open EVE Journal in browser")
    ];
}
