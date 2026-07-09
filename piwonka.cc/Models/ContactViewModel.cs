using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Bitte geben Sie Ihren Namen ein.")]
        [StringLength(100, ErrorMessage = "Der Name darf maximal 100 Zeichen lang sein.")]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte geben Sie Ihre E-Mail-Adresse ein.")]
        [EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein.")]
        [StringLength(200, ErrorMessage = "Die E-Mail-Adresse darf maximal 200 Zeichen lang sein.")]
        [Display(Name = "E-Mail")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Der Betreff darf maximal 200 Zeichen lang sein.")]
        [Display(Name = "Betreff")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Bitte geben Sie eine Nachricht ein.")]
        [StringLength(5000, ErrorMessage = "Die Nachricht darf maximal 5000 Zeichen lang sein.")]
        [Display(Name = "Nachricht")]
        public string Message { get; set; } = string.Empty;

        // Honeypot-Feld für Spam-Schutz (muss leer bleiben)
        public string? Website { get; set; }
    }
}
