using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public enum TicketTyp
    {
        Bug = 0,
        FeatureRequest = 1
    }

    public enum TicketStatus
    {
        Offen = 0,
        InBearbeitung = 1,
        Geschlossen = 2
    }

    public class Ticket
    {
        public int Id { get; set; }

        public TicketTyp Typ { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.Offen;

        [Required]
        [StringLength(200)]
        public string Titel { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Beschreibung { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string MelderName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string MelderEmail { get; set; } = string.Empty;

        [StringLength(45)]
        public string? IpAdresse { get; set; }

        [StringLength(2000)]
        public string? AdminNotiz { get; set; }

        public DateTime ErstelltAm { get; set; } = DateTime.Now;
    }
}
