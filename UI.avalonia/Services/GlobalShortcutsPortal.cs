using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace UI.avalonia.Services;

public sealed record PortalShortcut(string Id, string Description, string? PreferredTrigger);

/// <summary>
/// Registers system-wide shortcuts on Linux through the XDG <c>org.freedesktop.portal.GlobalShortcuts</c>
/// desktop portal (supported by KWin/KDE). The compositor owns the actual key binding — the user can
/// view/change it in their desktop's shortcut settings — and notifies us via the Activated signal.
/// Best-effort: if the portal is unavailable the app silently falls back to in-app shortcuts.
/// </summary>
public sealed class GlobalShortcutsPortal : IDisposable
{
    private const string Service = "org.freedesktop.portal.Desktop";
    private const string Path = "/org/freedesktop/portal/desktop";
    private const string Interface = "org.freedesktop.portal.GlobalShortcuts";

    private Connection? _connection;
    private IDisposable? _activatedMatch;
    private IDisposable? _responseMatch;

    public event Action<string>? Activated;

    public async Task StartAsync(IReadOnlyList<PortalShortcut> shortcuts)
    {
        try
        {
            var address = Address.Session;
            if (string.IsNullOrEmpty(address))
                return;

            _connection = new Connection(address);
            await _connection.ConnectAsync();

            _activatedMatch = await _connection.AddMatchAsync(
                new MatchRule { Type = MessageType.Signal, Interface = Interface, Member = "Activated", Path = Path },
                static (Message m, object? _) =>
                {
                    var reader = m.GetBodyReader();
                    reader.ReadObjectPath();      // session handle
                    return reader.ReadString();   // shortcut id
                },
                (Exception? ex, string shortcutId, object? _, object? __) =>
                {
                    if (ex == null)
                        Activated?.Invoke(shortcutId);
                },
                ObserverFlags.None, null, null, false);

            var sessionReady = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            _responseMatch = await _connection.AddMatchAsync(
                new MatchRule { Type = MessageType.Signal, Interface = "org.freedesktop.portal.Request", Member = "Response" },
                static (Message m, object? _) =>
                {
                    var reader = m.GetBodyReader();
                    reader.ReadUInt32(); // response code
                    var results = reader.ReadDictionaryOfStringToVariantValue();
                    return results.TryGetValue("session_handle", out var handle) ? handle.GetString() : null;
                },
                (Exception? ex, string? sessionHandle, object? _, object? __) =>
                {
                    if (ex == null && sessionHandle != null)
                        sessionReady.TrySetResult(sessionHandle);
                },
                ObserverFlags.None, null, null, false);

            CreateSession();

            var winner = await Task.WhenAny(sessionReady.Task, Task.Delay(5000));
            if (winner == sessionReady.Task)
                BindShortcuts(sessionReady.Task.Result, shortcuts);
        }
        catch
        {
            // Portal not available/supported: fall back to in-app shortcuts.
        }
    }

    private void CreateSession()
    {
        using var writer = _connection!.GetMessageWriter();
        writer.WriteMethodCallHeader(Service, Path, Interface, "CreateSession", "a{sv}", MessageFlags.None);

        var options = writer.WriteDictionaryStart();
        writer.WriteDictionaryEntryStart();
        writer.WriteString("handle_token");
        writer.WriteVariantString("ewbagent_create");
        writer.WriteDictionaryEntryStart();
        writer.WriteString("session_handle_token");
        writer.WriteVariantString("ewbagent");
        writer.WriteDictionaryEnd(options);

        _connection.TrySendMessage(writer.CreateMessage());
    }

    private void BindShortcuts(string sessionHandle, IReadOnlyList<PortalShortcut> shortcuts)
    {
        using var writer = _connection!.GetMessageWriter();
        writer.WriteMethodCallHeader(Service, Path, Interface, "BindShortcuts", "oa(sa{sv})sa{sv}", MessageFlags.None);
        writer.WriteObjectPath(sessionHandle);

        var array = writer.WriteArrayStart(DBusType.Struct);
        foreach (var shortcut in shortcuts)
        {
            writer.WriteStructureStart();
            writer.WriteString(shortcut.Id);

            var meta = writer.WriteDictionaryStart();
            writer.WriteDictionaryEntryStart();
            writer.WriteString("description");
            writer.WriteVariantString(shortcut.Description);
            if (!string.IsNullOrEmpty(shortcut.PreferredTrigger))
            {
                writer.WriteDictionaryEntryStart();
                writer.WriteString("preferred_trigger");
                writer.WriteVariantString(shortcut.PreferredTrigger);
            }
            writer.WriteDictionaryEnd(meta);
        }
        writer.WriteArrayEnd(array);

        writer.WriteString(string.Empty); // parent_window

        var options = writer.WriteDictionaryStart();
        writer.WriteDictionaryEnd(options);

        _connection.TrySendMessage(writer.CreateMessage());
    }

    public void Dispose()
    {
        _activatedMatch?.Dispose();
        _responseMatch?.Dispose();
        _connection?.Dispose();
    }
}
