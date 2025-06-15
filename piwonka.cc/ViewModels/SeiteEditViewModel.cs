using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Piwonka.CC.Models;

namespace Piwonka.CC.ViewModels
{
    public class SeiteEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        [StringLength(200)]
        public string Titel { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Die Sprache ist erforderlich.")]
        public Language Language { get; set; } = Language.DE;

        public DateTime ErstelltAm { get; set; }

        // Neue Menü-Eigenschaften
        [Display(Name = "Übergeordnete Seite")]
        public int? ParentId { get; set; }

        [StringLength(100)]
        [Display(Name = "Menü-Gruppe")]
        public string? MenuGruppe { get; set; }

        [StringLength(100)]
        [Display(Name = "Menü-Titel (wenn abweichend)")]
        public string? MenuTitel { get; set; }

        [Display(Name = "Ist Menü-Gruppe (ohne eigene Seite)")]
        public bool IstMenuGruppe { get; set; } = false;

        // Für Dropdown
        public List<SeiteOption> VerfuegbareParents { get; set; } = new();
    }
}