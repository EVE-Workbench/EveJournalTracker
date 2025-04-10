namespace SharedLibrary.Events;

public class CharacterStatusChangedEventArgs : EventArgs
{
    public long CharacterId { get; }
    public string Name { get; }
    public bool IsOnline { get; }

    public CharacterStatusChangedEventArgs(long characterId, string name, bool isOnline)
    {
        CharacterId = characterId;
        Name = name;
        IsOnline = isOnline;
    }
}