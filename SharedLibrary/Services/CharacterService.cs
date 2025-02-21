using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SharedLibrary.Services;

public class CharacterService
{
    public static async Task<string?> GetCharacterNameAsync(int charId)
    {
        using HttpClient client = new();
        client.BaseAddress = new Uri("https://esi.evetech.net/latest/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            Console.WriteLine($"Request url: characters/{charId}/?datasource=tranquility");
            var response = await client.GetAsync($"characters/{charId}/?datasource=tranquility");
            response.EnsureSuccessStatusCode(); // trigger exception if not successful

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("name").GetString();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }

        return null;
    }
}