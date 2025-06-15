using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class Kategorie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Beschreibung { get; set; }

        [Required]
        public Language Language { get; set; } = Language.DE;

        // Navigation Property für Posts
        public ICollection<Post> Posts { get; set; } = new List<Post>();

        [StringLength(100)]
        public string Slug { get; set; } = default!;

        public DateTime ErstelltAm { get; set; }

        // Helper-Methode für Sprach-Display
        public string GetLanguageDisplayName()
        {
            return Language switch
            {
                Language.DE => "Deutsch",
                Language.EN => "English",
                _ => "Unbekannt"
            };
        }

        // Helper-Methode für Sprach-Code
        public string GetLanguageCode()
        {
            return Language switch
            {
                Language.DE => "de",
                Language.EN => "en",
                _ => "unknown"
            };
        }
    }
}