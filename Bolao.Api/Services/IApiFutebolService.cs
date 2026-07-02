using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bolao.Api.Services
{
    public interface IApiFutebolService
    {
        Task<List<PartidaResultadoDTO>> BuscarPartidasFinalizadasDoDiaAnteriorAsync();
    }

    public class PartidaResultadoDTO
    {
        public int PartidaExternaId { get; set; }
        public int GolsCasa { get; set; }
        public int GolsFora { get; set; }
    }
}
