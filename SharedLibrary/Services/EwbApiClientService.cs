using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace SharedLibrary.Services;

public class EwbApiClientService
{
    public HttpClient HttpClient { get; }
    public ILogger<EwbApiClientService> Logger { get; }
    public IOptions<EwbApiOptions> Options { get; }
    
    public EwbApiClientService(HttpClient httpClient, ILogger<EwbApiClientService> logger, IOptions<EwbApiOptions> options)
    {
        
        HttpClient = httpClient;
        Logger = logger;
        Options = options;
    }
    
    public async Task<IEnumerable<Dungeon>?> GetDungeons(DungeonType type)
    {
        var response = await HttpClient.GetAsync($"{Options.Value.BaseUrl}/dungeons?type={type}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<Dungeon>>(content);
    }
}

public class EwbApiOptions
{
    public string BaseUrl { get; set; } = null!;
    
}