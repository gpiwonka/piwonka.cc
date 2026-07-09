using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Tickets
{
    public class CreateModel : PageModel
    {
        private const int MaxTicketsPerIpProStunde = 3;
        private const int MinSekundenAufFormular = 3;

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<CreateModel> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        [BindProperty]
        public TicketViewModel Ticket { get; set; } = new();

        public bool Submitted { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet(string? typ = null)
        {
            Ticket.FormGeneriertUm = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (string.Equals(typ, "feature", StringComparison.OrdinalIgnoreCase))
            {
                Ticket.Typ = TicketTyp.FeatureRequest;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Honeypot: Bots füllen das versteckte Feld aus
            if (!string.IsNullOrEmpty(Ticket.Website))
            {
                _logger.LogWarning("Ticket-Spam erkannt: Honeypot ausgefüllt (IP {Ip})", GetClientIp());
                Submitted = true;
                return Page();
            }

            // Time-Trap: Formular zu schnell abgeschickt
            var alterSekunden = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Ticket.FormGeneriertUm;
            if (Ticket.FormGeneriertUm <= 0 || alterSekunden < MinSekundenAufFormular)
            {
                _logger.LogWarning("Ticket-Spam erkannt: Formular nach {Sek}s abgeschickt (IP {Ip})", alterSekunden, GetClientIp());
                Submitted = true;
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var ip = GetClientIp();

            using var context = await _contextFactory.CreateDbContextAsync();

            // Ratelimit per IP (rollende letzte Stunde)
            if (!string.IsNullOrEmpty(ip))
            {
                var seit = DateTime.Now.AddHours(-1);
                var anzahl = await context.Tickets
                    .CountAsync(t => t.IpAdresse == ip && t.ErstelltAm >= seit);

                if (anzahl >= MaxTicketsPerIpProStunde)
                {
                    _logger.LogWarning("Ticket-Ratelimit erreicht für IP {Ip} ({Count} in letzter Stunde)", ip, anzahl);
                    HasError = true;
                    ErrorMessage = "Sie haben in der letzten Stunde bereits mehrere Tickets gesendet. Bitte versuchen Sie es später erneut.";
                    Ticket.FormGeneriertUm = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    return Page();
                }
            }

            try
            {
                var entity = new Ticket
                {
                    Typ = Ticket.Typ,
                    Status = TicketStatus.Offen,
                    Titel = Ticket.Titel.Trim(),
                    Beschreibung = Ticket.Beschreibung.Trim(),
                    MelderName = Ticket.MelderName.Trim(),
                    MelderEmail = Ticket.MelderEmail.Trim(),
                    IpAdresse = ip,
                    ErstelltAm = DateTime.Now
                };

                context.Tickets.Add(entity);
                await context.SaveChangesAsync();

                _logger.LogInformation("Neues Ticket #{Id} ({Typ}) von {Email}", entity.Id, entity.Typ, entity.MelderEmail);
                Submitted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern des Tickets");
                HasError = true;
                ErrorMessage = "Es ist ein Fehler beim Speichern aufgetreten. Bitte versuchen Sie es später erneut.";
                Ticket.FormGeneriertUm = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            return Page();
        }

        private string? GetClientIp()
        {
            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                return forwarded.Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
