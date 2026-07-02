using Microsoft.EntityFrameworkCore;
using Bolao.Api.Models;

namespace Bolao.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Partida> Partidas { get; set; } = null!;
        public DbSet<Aposta> Apostas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Garante que as tabelas sejam criadas com nomes no plural
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Partida>().ToTable("Partidas");
            modelBuilder.Entity<Aposta>().ToTable("Apostas");

            // Configuração dos relacionamentos de Aposta (Usuario e Partida)
            modelBuilder.Entity<Aposta>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.Apostas)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Aposta>()
                .HasOne(a => a.Partida)
                .WithMany(p => p.Apostas)
                .HasForeignKey(a => a.PartidaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração de valores padrão/default
            modelBuilder.Entity<Partida>()
                .Property(p => p.Status)
                .HasDefaultValue("Agendada");

            modelBuilder.Entity<Aposta>()
                .Property(a => a.PontosObtidos)
                .HasDefaultValue(0);

            modelBuilder.Entity<Aposta>()
                .Property(a => a.DataAposta)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Usado para PostgreSQL / SQLite / SQL Server generico. No PostgreSQL pode ser 'now()' ou 'CURRENT_TIMESTAMP' ou 'transaction_timestamp()'
        }
    }
}
