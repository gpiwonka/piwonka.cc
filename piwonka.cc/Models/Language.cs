using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class Language
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2)]
        public string Code { get; set; } = string.Empty; // "de", "en", "fr"

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // "Deutsch", "English", "Français"

        [StringLength(100)]
        public string NativeName { get; set; } = string.Empty; // "Deutsch", "English", "Français"

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [StringLength(10)]
        public string CultureCode { get; set; } = string.Empty; // "de-DE", "en-US", "fr-FR"

        public int SortOrder { get; set; } = 0;

        // Navigation Properties
     
    }
}