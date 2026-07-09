// Data/ApplicationDbContext.cs (aktualisieren)
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Models;

namespace Piwonka.CC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Kategorie> Kategorien { get; set; }
        public DbSet<Seite> Seiten { get; set; } // Neue DbSet für Seiten

        public DbSet<Analytics> Analytics { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<PlanAdviceResult> PlanAdviceResults { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<IndexFeature> IndexFeatures { get; set; }
        public DbSet<AboutCard> AboutCards { get; set; }
        public DbSet<DownloadApp> DownloadApps { get; set; }
        public DbSet<DownloadDatei> DownloadDateien { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Index für Slug
            modelBuilder.Entity<Seite>()
                .HasIndex(s => s.Slug)
                .IsUnique();

            modelBuilder.Entity<PlanAdviceResult>()
                .HasIndex(p => p.Hash)
                .IsUnique();

            // Beim Löschen einer App werden ihre Dateien mitgelöscht
            modelBuilder.Entity<DownloadDatei>()
                .HasOne(d => d.App)
                .WithMany(a => a.Dateien)
                .HasForeignKey(d => d.DownloadAppId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}