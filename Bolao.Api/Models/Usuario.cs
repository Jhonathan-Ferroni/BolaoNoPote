using System.Collections.Generic;

namespace Bolao.Api.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string? UrlFotoPerfil { get; set; }

        // Relacionamento: Um Usuário pode ter várias Apostas
        public ICollection<Aposta> Apostas { get; set; } = new List<Aposta>();
    }
}
