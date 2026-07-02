using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Bolao.Api.Services
{
    public class ApiFootballService : IApiFutebolService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ApiFootballService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            _httpClient.BaseAddress = new Uri(_configuration["ApiFootball:BaseUrl"]!);
            _httpClient.DefaultRequestHeaders.Add("x-apisports-key", _configuration["ApiFootball:ApiKey"]);
        }

        public async Task<List<PartidaResultadoDTO>> BuscarPartidasFinalizadasDoDiaAnteriorAsync()
        {
            // Nota: Esta é uma implementação simplificada baseada na estrutura solicitada.
            // A rota exata depende da documentação da API-Football.
            var dataAnterior = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<List<PartidaResultadoDTO>>($"fixtures?date={dataAnterior}&status=FT");
            
            return response ?? new List<PartidaResultadoDTO>();
        }
    }
}
