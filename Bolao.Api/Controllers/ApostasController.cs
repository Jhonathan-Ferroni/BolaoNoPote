using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bolao.Api.Data;
using Bolao.Api.DTOs;
using Bolao.Api.Models;

namespace Bolao.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApostasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApostasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostAposta([FromBody] NovaApostaDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return Unauthorized();
            }

            var partida = await _context.Partidas.FindAsync(dto.PartidaId);
            if (partida == null)
            {
                return NotFound("Partida não encontrada.");
            }

            if (partida.DataHora <= DateTime.UtcNow)
            {
                return BadRequest("Não é possível apostar em uma partida que já começou ou terminou.");
            }

            var aposta = new Aposta
            {
                UsuarioId = usuarioId,
                PartidaId = dto.PartidaId,
                PalpiteGolsCasa = dto.PalpiteGolsCasa,
                PalpiteGolsFora = dto.PalpiteGolsFora,
                DataAposta = DateTime.UtcNow
            };

            _context.Apostas.Add(aposta);
            await _context.SaveChangesAsync();

            return Ok(new { Mensagem = "Aposta realizada com sucesso!" });
        }

        [HttpGet("ranking")]
        public async Task<IActionResult> GetRanking()
        {
            var ranking = await _context.Apostas
                .Include(a => a.Usuario)
                .GroupBy(a => a.Usuario.Nome)
                .Select(g => new RankingDTO
                {
                    NomeUsuario = g.Key,
                    PontosTotais = g.Sum(a => a.PontosObtidos),
                    AcertosCravados = g.Count(a => a.PontosObtidos == 5)
                })
                .OrderByDescending(r => r.PontosTotais)
                .ThenByDescending(r => r.AcertosCravados)
                .ToListAsync();

            return Ok(ranking);
        }
    }
}
