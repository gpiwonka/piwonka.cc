using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class DownloadApp
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Beschreibung { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public int Reihenfolge { get; set; }

        public bool Aktiv { get; set; } = true;

        public Language Language { get; set; } = Language.DE;

        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        public virtual ICollection<DownloadDatei> Dateien { get; set; } = new List<DownloadDatei>();
    }
}
