namespace Bolao.Api.DTOs
{
    public class PartidaDTO
    {
        public int Id { get; set; }
        public string TimeCasa { get; set; } = string.Empty;
        public string TimeFora { get; set; } = string.Empty;
        public System.DateTime DataHora { get; set; }
    }
}
