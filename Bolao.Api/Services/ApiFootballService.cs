using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BolaoCopa.Api.Models;

namespace BolaoCopa.Api.Services;

public interface IApiFutebolService
{
    Task<List<PartidaExternaDto>> BuscarPartidasFinalizadasDoDia(DateTime data);
}

public class ApiFootballService : IApiFutebolService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ApiFootballService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        // Puxa as configurações do appsettings.json
        var apiKey = _configuration["ApiFootball:ApiKey"];
        var baseUrl = _configuration["ApiFootball:BaseUrl"];

        _httpClient.BaseAddress = new Uri(baseUrl!);
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
        // Ajusta o host automaticamente com base na URL fornecida
        var host = new Uri(baseUrl!).Host;
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", host);
    }

    public async Task<List<PartidaExternaDto>> BuscarPartidasFinalizadasDoDia(DateTime data)
    {
        string dataFormatada = data.ToString("yyyy-MM-dd");
        
        // Ajuste o endpoint conforme a documentação da SportAPI para buscar por data
        // Exemplo genérico usando os parâmetros comuns deles:
        var url = $"events/date/{dataFormatada}"; 

        try
        {
            var resposta = await _httpClient.GetFromJsonAsync<SportApiResponse>(url);

            if (resposta?.Events == null) return new List<PartidaExternaDto>();

            // Mapeia o JSON da SportAPI para o DTO que o nosso Worker consome
            return resposta.Events
                .Where(e => e.Status?.Type == "finished") // Só processa jogos que já acabaram
                .Select(e => new PartidaExternaDto
                {
                    PartidaExternaId = e.Id,
                    TimeCasa = e.HomeTeam?.Name ?? "Desconhecido",
                    TimeFora = e.AwayTeam?.Name ?? "Desconhecido",
                    DataHora = DateTimeOffset.FromUnixTimeSeconds(e.StartTimestamp).UtcDateTime,
                    GolsCasa = e.HomeScore?.Current,
                    GolsFora = e.AwayScore?.Current,
                    Status = "Finalizada"
                }).ToList();
        }
        catch (Exception)
        {
            // Evita que falhas na API externa derrubem o Worker de 12 horas do .NET
            return new List<PartidaExternaDto>();
        }
    }
}

// --- DTOs de Mapeamento da SportAPI ---

public class SportApiResponse
{
    [JsonPropertyName("events")]
    public List<SportApiEvent>? Events { get; set; }
}

public class SportApiEvent
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("startTimestamp")]
    public long StartTimestamp { get; set; }

    [JsonPropertyName("status")]
    public SportApiStatus? Status { get; set; }

    [JsonPropertyName("homeTeam")]
    public TeamData? HomeTeam { get; set; }

    [JsonPropertyName("awayTeam")]
    public TeamData? AwayTeam { get; set; }

    [JsonPropertyName("homeScore")]
    public ScoreData? HomeScore { get; set; }

    [JsonPropertyName("awayScore")]
    public ScoreData? AwayScore { get; set; }
}

public class SportApiStatus
{
    [JsonPropertyName("type")]
    public string? Type { get; set; } // finished, inprogress, notstarted
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class TeamData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class ScoreData
{
    [JsonPropertyName("current")]
    public int? Current { get; set; }
}