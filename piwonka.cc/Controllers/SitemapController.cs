using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

namespace Piwonka.CC.Controllers
{
    [ApiController]
    public class SitemapController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SitemapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("/sitemap.xml")]
        [HttpGet]
        [Produces("application/xml")]
        public async Task<ContentResult> Index()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var sitemapXml = await GenerateSitemapAsync(baseUrl);

                return new ContentResult
                {
                    Content = sitemapXml,
                    ContentType = "application/xml; charset=utf-8",
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                // Fallback bei Fehlern
                var minimalSitemap = GenerateMinimalSitemap($"{Request.Scheme}://{Request.Host}");
                return new ContentResult
                {
                    Content = minimalSitemap,
                    ContentType = "application/xml; charset=utf-8",
                    StatusCode = 200
                };
            }
        }

        private async Task<string> GenerateSitemapAsync(string baseUrl)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Homepage
            AppendUrl(sb, baseUrl, "/", DateTime.UtcNow, "daily", "1.0");

            // Blog-Hauptseite
            var latestBlogUpdate = await _context.Posts
                .Where(p => p.IstVeroeffentlicht)
                .OrderByDescending(p => p.ErstelltAm)
                .Select(p => p.ErstelltAm)
                .FirstOrDefaultAsync();

            AppendUrl(sb, baseUrl, "/blog",
                latestBlogUpdate != default ? latestBlogUpdate : DateTime.UtcNow,
                "daily", "0.9");

            // Suchseite
            AppendUrl(sb, baseUrl, "/search", DateTime.UtcNow, "monthly", "0.5");

            // Seiten
            var seiten = await _context.Seiten
                .Where(s => s.IstVeroeffentlicht)
                .ToListAsync();

            foreach (var seite in seiten)
            {
                var lastMod = seite.BearbeitetAm ?? seite.ErstelltAm;
                var changeFreq = GetChangeFrequency(seite.Template);
                var priority = GetPriority(seite.Template, seite.ImMenuAnzeigen);

                AppendUrl(sb, baseUrl, $"/seite/{seite.Slug}", lastMod, changeFreq, priority);
            }

            // Blog-Posts
            var posts = await _context.Posts
                .Where(p => p.IstVeroeffentlicht)
                .ToListAsync();

            foreach (var post in posts)
            {
                AppendUrl(sb, baseUrl, $"/blog/{post.Slug}", post.ErstelltAm, "monthly", "0.7");
            }

            // Kategorien
            var kategorien = await _context.Kategorien
                .Where(k => k.Posts.Any(p => p.IstVeroeffentlicht))
                .ToListAsync();

            foreach (var kategorie in kategorien)
            {
                AppendUrl(sb, baseUrl, $"/blog/kategorie/{kategorie.Slug}", kategorie.ErstelltAm, "weekly", "0.6");
            }

            sb.AppendLine("</urlset>");
            return sb.ToString();
        }

        private void AppendUrl(StringBuilder sb, string baseUrl, string location, DateTime lastMod, string changeFreq, string priority)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}{location}</loc>");
            sb.AppendLine($"    <lastmod>{lastMod:yyyy-MM-dd}</lastmod>");
            sb.AppendLine($"    <changefreq>{changeFreq}</changefreq>");
            sb.AppendLine($"    <priority>{priority}</priority>");
            sb.AppendLine("  </url>");
        }

        private string GetChangeFrequency(string template)
        {
            return template switch
            {
                "Kontakt" => "monthly",
                "Standard" => "weekly",
                "Vollbreite" => "weekly",
                _ => "monthly"
            };
        }

        private string GetPriority(string template, bool imMenu)
        {
            if (imMenu)
            {
                return template switch
                {
                    "Kontakt" => "0.8",
                    "Standard" => "0.7",
                    "Vollbreite" => "0.7",
                    _ => "0.6"
                };
            }
            return "0.5";
        }

        private string GenerateMinimalSitemap(string baseUrl)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
  <url>
    <loc>{baseUrl}/</loc>
    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>
    <changefreq>daily</changefreq>
    <priority>1.0</priority>
  </url>
</urlset>";
        }
    }
}