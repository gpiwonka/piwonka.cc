using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class TicketViewModel
    {
        [Required(ErrorMessage = "Bitte wählen Sie eine Kategorie.")]
        [Display(Name = "Kategorie")]
        public TicketTyp Typ { get; set; } = TicketTyp.Bug;

        [Required(ErrorMessage = "Bitte geben Sie einen Titel ein.")]
        [StringLength(200, ErrorMessage = "Der Titel darf maximal 200 Zeichen lang sein.")]
        [Display(Name = "Titel")]
        public string Titel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte beschreiben Sie das Problem oder den Wunsch.")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Die Beschreibung muss zwischen 10 und 5000 Zeichen lang sein.")]
        [Display(Name = "Beschreibung")]
        public string Beschreibung { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte geben Sie Ihren Namen ein.")]
        [StringLength(100, ErrorMessage = "Der Name darf maximal 100 Zeichen lang sein.")]
        [Display(Name = "Name")]
        public string MelderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte geben Sie Ihre E-Mail-Adresse ein.")]
        [EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein.")]
        [StringLength(200, ErrorMessage = "Die E-Mail-Adresse darf maximal 200 Zeichen lang sein.")]
        [Display(Name = "E-Mail")]
        public string MelderEmail { get; set; } = string.Empty;

        public string? Website { get; set; }

        public long FormGeneriertUm { get; set; }
    }
}
