using System;

namespace Bolao.Api.Models
{
    public class Aposta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PartidaId { get; set; }
        public int PalpiteGolsCasa { get; set; }
        public int PalpiteGolsFora { get; set; }
        public int PontosObtidos { get; set; } = 0;
        public DateTime DataAposta { get; set; } = DateTime.UtcNow;

        // Propriedades de Navegação
        public Usuario Usuario { get; set; } = null!;
        public Partida Partida { get; set; } = null!;
    }
}
