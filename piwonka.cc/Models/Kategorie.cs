using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class Kategorie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Beschreibung { get; set; }

        // NEU: Sprache hinzufügen
        [Required]
        [StringLength(5)]
        [Display(Name = "Sprache")]
        public string Sprache { get; set; } = "de"; // Standard: Deutsch

        // Navigation Property für Posts
        public ICollection<Post> Posts { get; set; }
    }
}
