using SharedLibrary.Models;

namespace SharedLibrary.Repositories;

public class CharacterRepository
{
    private static readonly Lazy<CharacterRepository> _instance = new Lazy<CharacterRepository>(() => new CharacterRepository());
    private readonly Dictionary<int, Character> _characters;

    private CharacterRepository()
    {
        _characters = new Dictionary<int, Character>();
    }

    public static CharacterRepository Instance => _instance.Value;

    public Dictionary<int, Character> Characters => _characters;

    public event Action<Character> CharacterAdded;

    public Character GetOrCreateCharacter(int characterId, Func<int, Character> createCharacter)
    {
        if (!_characters.TryGetValue(characterId, out var character))
        {
            character = createCharacter(characterId);
            _characters[characterId] = character;
            CharacterAdded?.Invoke(character);
        }
        return character;
    }
}