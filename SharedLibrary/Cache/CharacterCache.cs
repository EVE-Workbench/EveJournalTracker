using System.Collections.Concurrent;
using SharedLibrary.Data;
using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Cache;

public class CharacterCache
{
    private readonly ConcurrentDictionary<int, Character> _characters;
    private readonly AppDbContext _context;

    public event EventHandler<Character>? CharacterAdded = null;

    public CharacterCache(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _characters = new ConcurrentDictionary<int, Character>();

        LoadCharactersFromDatabase();
    }

    public List<Character> GetAllCharacters()
    {
        return _characters.Values.ToList();
    }

    private void LoadCharactersFromDatabase()
    {
        var charactersFromDb = _context.Characters.AsNoTracking().ToList();

        foreach (var character in charactersFromDb)
        {
            _characters[character.CharacterId] = Character.FromDto(character);
        }
    }

    public Character? GetCharacter(int characterId)
    {
        _characters.TryGetValue(characterId, out var character);
        return character;
    }

    public void AddCharacter(Character character)
    {
        _characters[character.CharacterId] = character;
        CharacterAdded?.Invoke(this, character);
        //_context.Add(character.ToDto());
    }

    public void RemoveCharacter(int characterId)
    {
        if (_characters.TryRemove(characterId, out var character))
        {
            _context.Remove(character.ToDto());
        }
    }

    public void SaveChanges()
    {
        var characterDtos =  _context.Characters.ToList();
        
        
        foreach (var character in _characters.Values)
        {
            var existingCharacterDto = characterDtos.FirstOrDefault(c => c.CharacterId == character.CharacterId);
            var characterDto = character.ToDto(existingCharacterDto);

            if (existingCharacterDto == null)
            {
                _context.Add(characterDto);
            }
        }
        
        _context.SaveChanges();
    }
}