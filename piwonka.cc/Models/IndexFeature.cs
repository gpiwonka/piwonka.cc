using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class IndexFeature
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titel { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Beschreibung { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Link { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public int Reihenfolge { get; set; }

        public bool Aktiv { get; set; } = true;

        public Language Language { get; set; } = Language.DE;

        public DateTime ErstelltAm { get; set; } = DateTime.Now;
    }
}
