using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Piwonka.CC.Models
{
    public class Seite
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titel { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        public string? Inhalt { get; set; }

        [StringLength(200)]
        public string? MetaDescription { get; set; }

        [StringLength(500)]
        public string? MetaKeywords { get; set; }

        public bool IstVeroeffentlicht { get; set; } = true;

        public bool ImMenuAnzeigen { get; set; } = false;

        public int Reihenfolge { get; set; } = 0;

        [StringLength(50)]
        public string Template { get; set; } = "Standard";

        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        public DateTime? BearbeitetAm { get; set; }

        // Neue Eigenschaften für Menü-Hierarchie
        public int? ParentId { get; set; }

        [StringLength(100)]
        public string? MenuGruppe { get; set; }

        [StringLength(100)]
        public string? MenuTitel { get; set; }

        public bool IstMenuGruppe { get; set; } = false;

        // Navigation Properties
        [ForeignKey("ParentId")]
        public virtual Seite? Parent { get; set; }

        public virtual Language Language { get; set; } = Language.DE;

        public virtual ICollection<Seite> Children { get; set; } = new List<Seite>();
    }
}