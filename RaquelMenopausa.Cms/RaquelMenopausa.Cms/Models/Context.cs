using Microsoft.EntityFrameworkCore;
using Serilog;

namespace RaquelMenopausa.Cms.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        public Context()
        {
        }
        public DbSet<UsuarioModuloPermissao> UsuarioModuloPermissoes { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Logs> Logs { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioLog> UsuarioLogs { get; set; }
        public DbSet<ModuloPermissao> ModuloPermissoes { get; set; }
        public DbSet<Permissao> Permissoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ModuloPermissao>()
            //    .HasOne(um => um.Modulo)
            //    .WithMany(m => m.ModuloPermissao)
            //    .HasForeignKey(um => um.ModuloId);

            //modelBuilder.Entity<ModuloPermissao>()
            //    .HasOne(um => um.Permissao)
            //    .WithMany(um => um.ModuloPermissao)
            //    .HasForeignKey(um => um.PermissaoId);
        }

    }
}
