using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bolao.Api.Data;
using Bolao.Api.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Api.Services
{
    public class AtualizacaoResultadosWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AtualizacaoResultadosWorker> _logger;

        public AtualizacaoResultadosWorker(IServiceProvider serviceProvider, ILogger<AtualizacaoResultadosWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Iniciando processamento de resultados: {time}", DateTimeOffset.Now);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var apiService = scope.ServiceProvider.GetRequiredService<IApiFutebolService>();

                    try
                    {
                        var partidasFinalizadas = await apiService.BuscarPartidasFinalizadasDoDiaAnteriorAsync();

                        foreach (var partidaApi in partidasFinalizadas)
                        {
                            var partida = await context.Partidas.Include(p => p.Apostas).FirstOrDefaultAsync(p => p.PartidaExternaId == partidaApi.PartidaExternaId);
                            
                            if (partida != null && partida.Status != "Finalizada")
                            {
                                partida.GolsCasa = partidaApi.GolsCasa;
                                partida.GolsFora = partidaApi.GolsFora;
                                partida.Status = "Finalizada";

                                foreach (var aposta in partida.Apostas)
                                {
                                    int pontos = CalcularPontos(aposta.PalpiteGolsCasa, aposta.PalpiteGolsFora, partidaApi.GolsCasa, partidaApi.GolsFora);
                                    aposta.PontosObtidos = pontos;

                                    string motivo = pontos == 5 ? "Cravou" : (pontos == 3 ? "Tendência" : "Erro total");
                                    _logger.LogInformation("Usuário {UsuarioId} apostou {GolsCasa}x{GolsFora}, real foi {RealGolsCasa}x{RealGolsFora}. Pontos: {Pontos}. Motivo: {Motivo}", 
                                        aposta.UsuarioId, aposta.PalpiteGolsCasa, aposta.PalpiteGolsFora, partidaApi.GolsCasa, partidaApi.GolsFora, pontos, motivo);
                                }
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar resultados.");
                    }
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private int CalcularPontos(int palpiteCasa, int palpiteFora, int realCasa, int realFora)
        {
            if (palpiteCasa == realCasa && palpiteFora == realFora) return 5;
            
            bool palpiteVenceCasa = palpiteCasa > palpiteFora;
            bool palpiteVenceFora = palpiteFora > palpiteCasa;
            bool palpiteEmpate = palpiteCasa == palpiteFora;

            bool realVenceCasa = realCasa > realFora;
            bool realVenceFora = realFora > realCasa;
            bool realEmpate = realCasa == realFora;

            if ((palpiteVenceCasa && realVenceCasa) || (palpiteVenceFora && realVenceFora) || (palpiteEmpate && realEmpate)) return 3;

            return 0;
        }
    }
}
