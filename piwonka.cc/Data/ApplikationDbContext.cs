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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Index für Slug
            modelBuilder.Entity<Seite>()
                .HasIndex(s => s.Slug)
                .IsUnique();

            // Beispiel-Kategorien
            modelBuilder.Entity<Kategorie>().HasData(
                new Kategorie { Id = 1, Name = "Allgemein", Beschreibung = "Allgemeine Beiträge" },
                new Kategorie { Id = 2, Name = "Technologie", Beschreibung = "Beiträge über Technologie" },
                new Kategorie { Id = 3, Name = "Lifestyle", Beschreibung = "Beiträge über Lifestyle" }
            );

           
        }
    }
}