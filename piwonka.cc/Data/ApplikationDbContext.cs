using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Models;


namespace Piwonka.CC.Data
{
    
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

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Beispiel-Kategorien erstellen
                modelBuilder.Entity<Kategorie>().HasData(
                    new Kategorie { Id = 1, Name = "Allgemein", Beschreibung = "Allgemeine Beiträge" },
                    new Kategorie { Id = 2, Name = "Technologie", Beschreibung = "Beiträge über Technologie" },
                    new Kategorie { Id = 3, Name = "Lifestyle", Beschreibung = "Beiträge über Lifestyle" }
                );
            }
        }
    }
}
