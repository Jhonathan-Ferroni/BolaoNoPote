using System;
using System.Collections.Generic;

namespace Bolao.Api.Models
{
    public class Partida
    {
        public int Id { get; set; }
        public int PartidaExternaId { get; set; }
        public string TimeCasa { get; set; } = string.Empty;
        public string TimeFora { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
        public int? GolsCasa { get; set; }
        public int? GolsFora { get; set; }
        public string Status { get; set; } = "Agendada";

        // Relacionamento: Uma Partida pode ter várias Apostas
        public ICollection<Aposta> Apostas { get; set; } = new List<Aposta>();
    }
}
