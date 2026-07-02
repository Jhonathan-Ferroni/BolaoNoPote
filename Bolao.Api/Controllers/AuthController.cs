using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Bolao.Api.Data;
using Bolao.Api.DTOs;
using Bolao.Api.Models;
// NOTA: Certifique-se de que o pacote NuGet 'BCrypt.Net-Next' esteja instalado para rodar o código abaixo.
using BCryptNet = BCrypt.Net.BCrypt;

namespace Bolao.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroUsuarioDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Senha))
            {
                return BadRequest("Nome e senha são obrigatórios.");
            }

            // Verifica se o usuário já existe
            var usuarioExistente = await _context.Usuarios.AnyAsync(u => u.Nome.ToLower() == dto.Nome.ToLower());
            if (usuarioExistente)
            {
                return BadRequest("Já existe um usuário cadastrado com este nome.");
            }

            // Hashing da senha usando o BCrypt.Net-Next
            // Comentário para lembrar da instalação do pacote: BCrypt.Net-Next é instalado via NuGet
            string senhaHash = BCryptNet.HashPassword(dto.Senha);

            var novoUsuario = new Usuario
            {
                Nome = dto.Nome,
                SenhaHash = senhaHash,
                UrlFotoPerfil = dto.UrlFotoPerfil
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { Mensagem = "Usuário registrado com sucesso!", UsuarioId = novoUsuario.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Senha))
            {
                return BadRequest("Nome e senha são obrigatórios.");
            }

            // Busca o usuário pelo nome
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nome.ToLower() == dto.Nome.ToLower());
            if (usuario == null)
            {
                return Unauthorized("Usuário ou senha incorretos.");
            }

            // Valida a senha usando BCrypt
            bool senhaValida = BCryptNet.Verify(dto.Senha, usuario.SenhaHash);
            if (!senhaValida)
            {
                return Unauthorized("Usuário ou senha incorretos.");
            }

            // Geração do token JWT assinado com a chave do appsettings.json
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                return StatusCode(500, "Erro interno: Chave JWT não configurada no servidor.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token expira em 7 dias
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                Usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.UrlFotoPerfil
                }
            });
        }
    }
}
