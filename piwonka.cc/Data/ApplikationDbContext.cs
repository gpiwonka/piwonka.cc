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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Index für Slug
            modelBuilder.Entity<Seite>()
                .HasIndex(s => s.Slug)
                .IsUnique();

           
        }
    }
}