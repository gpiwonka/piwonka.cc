using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Piwonka.CC.Models
{
    public class DownloadDatei
    {
        public int Id { get; set; }

        public int DownloadAppId { get; set; }

        [Required]
        [StringLength(100)]
        public string Plattform { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Version { get; set; }

        [StringLength(300)]
        public string? Hinweis { get; set; }

        // Pfad/URL zur per FTP hochgeladenen Datei, z. B. /downloads/nova/nova-win.zip
        [Required]
        [StringLength(500)]
        public string DateiPfad { get; set; } = string.Empty;

        // Freitext, da manuell gepflegt (Upload per FTP), z. B. "24 MB"
        [StringLength(50)]
        public string? Dateigroesse { get; set; }

        public DateTime? VeroeffentlichtAm { get; set; }

        public int Reihenfolge { get; set; }

        public bool Aktiv { get; set; } = true;

        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        [ForeignKey(nameof(DownloadAppId))]
        public virtual DownloadApp? App { get; set; }
    }
}
