using Piwonka.CC.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Piwonka.CC.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titel { get; set; }

        [Required]
        public string Inhalt { get; set; }

        [StringLength(100)]
        public string Slug { get; set; }

        [DataType(DataType.Date)]
        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        public string? BildUrl { get; set; }

        public bool IstVeroeffentlicht { get; set; } = false;

        // NEU: Sprache hinzufügen
        [Required]
        [StringLength(5)]
        [Display(Name = "Sprache")]
        public string Sprache { get; set; } = "de"; // Standard: Deutsch

        // Fremdschlüssel für Kategorie
        public int? KategorieId { get; set; }

        // Navigation Property für Kategorie
        [ForeignKey("KategorieId")]
        public Kategorie Kategorie { get; set; }

        [NotMapped]
        public IFormFile UploadedImage { get; set; } = default!;
    }
}