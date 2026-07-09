using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class AboutCard
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titel { get; set; } = string.Empty;

        [Required]
        public string Inhalt { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Icon { get; set; }

        public bool Aktiv { get; set; } = true;

        public Language Language { get; set; } = Language.DE;

        public DateTime ErstelltAm { get; set; } = DateTime.Now;
    }
}
