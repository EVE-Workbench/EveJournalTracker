using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SharedLibrary.Enums;
using SharedLibrary.Models.Api;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Services;

public class EwbApiClientService
{
    private readonly IConfiguration _configuration;
    public HttpClient HttpClient { get; }
    public IOptions<EwbApiOptions> Options { get; }
    
    public EwbApiClientService(HttpClient httpClient, IOptions<EwbApiOptions> options, IConfiguration configuration)
    {
        _configuration = configuration;

        HttpClient = httpClient;
        Options = options;
        HttpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? string.Empty);
    }
    
    public async Task<List<EveSystemDto>?> GetEveSystems()
    {
        var response = await HttpClient.GetAsync("/v1/solarsystems");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<EveSystemDto>>(content);
    }
    
    public async Task<IEnumerable<DungeonResponse>?> GetDungeonsByType(DungeonType type)
    {
        var response = await HttpClient.GetAsync($"/v1/dungeons?type={type}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<DungeonResponse>>(content);
    }
}

public class EwbApiOptions
{
    public string BaseUrl { get; set; } = null!;
    
}