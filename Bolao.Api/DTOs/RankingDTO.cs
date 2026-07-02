namespace Bolao.Api.DTOs
{
    public class RankingDTO
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public int PontosTotais { get; set; }
        public int AcertosCravados { get; set; }
    }
}
