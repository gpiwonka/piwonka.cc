using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Piwonka.CC.Models;
using System.Net;
using System.Net.Mail;

namespace Piwonka.CC.Pages
{
    public class ContactModel : PageModel
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly SiteSettings _site;
        private readonly ILogger<ContactModel> _logger;

        public ContactModel(IOptions<SmtpSettings> smtpSettings, SiteSettings site, ILogger<ContactModel> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _site = site;
            _logger = logger;
        }

        [BindProperty]
        public ContactViewModel Contact { get; set; } = new();

        public bool MessageSent { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Honeypot-Check: Wenn das versteckte Feld ausgefüllt ist, ist es wahrscheinlich ein Bot
            if (!string.IsNullOrEmpty(Contact.Website))
            {
                _logger.LogWarning("Spam-Versuch erkannt: Honeypot-Feld ausgefüllt");
                // Zeige Erfolgsmeldung, damit der Bot denkt es hat funktioniert
                MessageSent = true;
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await SendEmailAsync();
                MessageSent = true;
                _logger.LogInformation("Kontaktformular erfolgreich gesendet von {Email}", Contact.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Senden der Kontakt-E-Mail");
                HasError = true;
                ErrorMessage = "Es ist ein Fehler beim Senden aufgetreten. Bitte versuchen Sie es später erneut oder kontaktieren Sie uns direkt per E-Mail.";
            }

            return Page();
        }

        private async Task SendEmailAsync()
        {
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = true
            };

            var subject = string.IsNullOrEmpty(Contact.Subject)
                ? $"Kontaktanfrage von {Contact.Name}"
                : $"Kontaktanfrage: {Contact.Subject}";

            var body = $@"Neue Kontaktanfrage über {_site.Name}

Name: {Contact.Name}
E-Mail: {Contact.Email}
Betreff: {Contact.Subject ?? "(kein Betreff)"}

Nachricht:
{Contact.Message}

---
Gesendet am: {DateTime.Now:dd.MM.yyyy HH:mm}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mailMessage.To.Add(_smtpSettings.ToEmail);
            mailMessage.ReplyToList.Add(new MailAddress(Contact.Email, Contact.Name));

            await client.SendMailAsync(mailMessage);
        }
    }
}
