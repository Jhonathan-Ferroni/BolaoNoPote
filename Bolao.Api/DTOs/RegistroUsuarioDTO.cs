namespace Bolao.Api.DTOs
{
    public class RegistroUsuarioDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string? UrlFotoPerfil { get; set; }
    }
}
