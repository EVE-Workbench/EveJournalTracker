using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SharedLibrary.Cache;
using SharedLibrary.Models;

namespace SharedLibrary.Services;

public class CharacterService
{
    private readonly CharacterCache _characterCache;
    private const string BaseUrl = "https://esi.evetech.net/latest/";

    public CharacterService(CharacterCache characterCache)
    {
        _characterCache = characterCache;
    }

    public Character GetOrCreateCharacter(int characterId)
    {
        var character = _characterCache.GetCharacter(characterId);
        if (character != null)
        {
            return character;
        }

        // Get the character name from the ESI API
        var characterName = GetCharacterNameAsync(characterId).GetAwaiter().GetResult();

        var newCharacter = new Character
        {
            CharacterId = characterId,
            Name = characterName ?? $"Char-{characterId}"
        };

        _characterCache.AddCharacter(newCharacter);

        return newCharacter;
    }
    
    public async Task<string?> GetCharacterNameAsync(int charId)
    {
        var json = await EsiQueryAsync(
            $"characters/{charId}/?datasource=tranquility",
            HttpMethod.Get);

        return json?.RootElement.GetProperty("name").GetString();
    }
    
    public async Task<int?> GetCharacterIdAsync(string charName)
    {
        var body = JsonSerializer.Serialize(new[] { charName });
        var json = await EsiQueryAsync(
            "universe/ids/?datasource=tranquility",
            HttpMethod.Post,
            body);

        if (json == null) return null;

        if (json.RootElement.TryGetProperty("characters", out var charsElem)
            && charsElem.GetArrayLength() > 0)
        {
            return charsElem[0].GetProperty("id").GetInt32();
        }

        return null;
    }

    /**
     * Queries the ESI API for the given endpoint using the specified HTTP method.
     * Returns a JsonDocument if successful, or null if there was an error.
     */
    private async Task<JsonDocument?> EsiQueryAsync(
        string endpoint,
        HttpMethod method,
        string? body = null)
    {
        using HttpClient client = new();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "Eve Workbench - Agent");

        try
        {
            HttpRequestMessage request = new(method, endpoint);

            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseString);
        }
        catch
        {
            return null;
        }
    }
}