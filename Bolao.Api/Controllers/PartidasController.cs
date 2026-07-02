using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bolao.Api.Data;
using Bolao.Api.DTOs;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Bolao.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartidasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PartidasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("proximas")]
        public async Task<IActionResult> GetProximas()
        {
            var agora = DateTime.UtcNow;

            var partidas = await _context.Partidas
                .Where(p => p.DataHora > agora)
                .OrderBy(p => p.DataHora)
                .Select(p => new PartidaDTO
                {
                    Id = p.Id,
                    TimeCasa = p.TimeCasa,
                    TimeFora = p.TimeFora,
                    DataHora = p.DataHora
                })
                .ToListAsync();

            return Ok(partidas);
        }
    }
}
