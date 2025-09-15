using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Yourcode.Core.Cms.Models
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
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<Curriculo> Curriculos { get; set; }
        public DbSet<FAQ> Faqs { get; set; }
        public DbSet<Depoimento> Depoimentos { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogCategoria> BlogCategorias { get; set; }
        public DbSet<BlogImagem> BlogImagens { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Popup> Popups { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Contato> Contatos { get; set; }
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
