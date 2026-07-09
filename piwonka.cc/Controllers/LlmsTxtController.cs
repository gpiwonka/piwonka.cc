using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using System.Text;

namespace Piwonka.CC.Controllers
{
    [ApiController]
    public class LlmsTxtController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SiteSettings _site;

        public LlmsTxtController(ApplicationDbContext context, SiteSettings site)
        {
            _context = context;
            _site = site;
        }

        [Route("/llms.txt")]
        [HttpGet]
        [Produces("text/markdown")]
        public async Task<ContentResult> Index()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var content = await BuildAsync(baseUrl);

            return new ContentResult
            {
                Content = content,
                ContentType = "text/markdown; charset=utf-8",
                StatusCode = 200
            };
        }

        private async Task<string> BuildAsync(string baseUrl)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# {_site.Name}");
            sb.AppendLine();
            sb.AppendLine($"> {_site.LlmsSummary}");
            sb.AppendLine();
            sb.AppendLine("Die Site ist zweisprachig (Deutsch/Englisch). Standardsprache ist Deutsch.");
            sb.AppendLine();

            sb.AppendLine("## Über mich");
            sb.AppendLine();
            sb.AppendLine($"- Name: {_site.Author}");
            sb.AppendLine($"- Rolle: {_site.JobTitle}");
            sb.AppendLine($"- Standort: {_site.City}, {_site.CountryName}");
            sb.AppendLine($"- Kontakt: [Kontaktformular]({baseUrl}/Contact), E-Mail: {_site.ContactEmail}");
            sb.AppendLine("- Themen: SQL Server, Datenbankoptimierung, Query Performance, Microsoft .NET, ASP.NET Core");
            sb.AppendLine();

            sb.AppendLine("## SQL Tools (kostenlos im Browser)");
            sb.AppendLine();
            sb.AppendLine($"- [Plan Advice]({baseUrl}/PlanAdvice): SQL-Server-Ausführungspläne mit Claude analysieren lassen — bilingual (DE/EN).");
            sb.AppendLine($"- [Query Formatter]({baseUrl}/Tools/QueryFormatter): SQL-Code formatieren.");
            sb.AppendLine($"- [Index Advisor]({baseUrl}/Tools/IndexAdvisor): Index-Empfehlungen für SQL-Statements.");
            sb.AppendLine($"- [Deadlock Analyzer]({baseUrl}/Tools/DeadlockAnalyzer): SQL-Server-Deadlock-Reports auswerten.");
            sb.AppendLine($"- [Query Converter]({baseUrl}/Tools/QueryConverter): SQL zwischen Dialekten umwandeln.");
            sb.AppendLine();

            sb.AppendLine("## Blog");
            sb.AppendLine();
            sb.AppendLine($"- [Blog-Übersicht]({baseUrl}/blog): alle Artikel.");

            try
            {
                var posts = await _context.Posts
                    .Where(p => p.IstVeroeffentlicht && p.Language == Language.DE)
                    .OrderByDescending(p => p.ErstelltAm)
                    .Take(20)
                    .Select(p => new { p.Id, p.Slug, p.Titel, p.Excerpt, p.ErstelltAm })
                    .ToListAsync();

                if (posts.Any())
                {
                    sb.AppendLine();
                    sb.AppendLine("### Aktuelle Beiträge");
                    sb.AppendLine();
                    foreach (var p in posts)
                    {
                        var excerpt = !string.IsNullOrWhiteSpace(p.Excerpt) ? $" — {p.Excerpt.Trim()}" : "";
                        sb.AppendLine($"- [{p.Titel}]({baseUrl}/blog/{p.Id}/{p.Slug}) ({p.ErstelltAm:yyyy-MM-dd}){excerpt}");
                    }
                }
            }
            catch
            {
                // Bei DB-Problemen Liste einfach weglassen
            }

            try
            {
                var seiten = await _context.Seiten
                    .Where(s => s.IstVeroeffentlicht && s.Language == Language.DE)
                    .OrderBy(s => s.Reihenfolge)
                    .ThenBy(s => s.Titel)
                    .Select(s => new { s.Slug, s.Titel, s.MetaDescription })
                    .ToListAsync();

                if (seiten.Any())
                {
                    sb.AppendLine();
                    sb.AppendLine("## Seiten");
                    sb.AppendLine();
                    foreach (var s in seiten)
                    {
                        var desc = !string.IsNullOrWhiteSpace(s.MetaDescription) ? $" — {s.MetaDescription.Trim()}" : "";
                        sb.AppendLine($"- [{s.Titel}]({baseUrl}/seite/{s.Slug}){desc}");
                    }
                }
            }
            catch
            {
                // ignore
            }

            sb.AppendLine();
            sb.AppendLine("## Optional");
            sb.AppendLine();
            sb.AppendLine($"- [Sitemap (XML)]({baseUrl}/sitemap.xml)");
            sb.AppendLine($"- [Kontakt]({baseUrl}/Contact)");
            sb.AppendLine($"- [Bug melden / Feature wünschen]({baseUrl}/Tickets/Create)");

            return sb.ToString();
        }
    }
}
