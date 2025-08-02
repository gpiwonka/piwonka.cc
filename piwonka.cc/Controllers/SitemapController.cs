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
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var urlElements = new List<XElement>();

            // Homepage
            urlElements.Add(CreateUrlElement(xmlns, baseUrl, "/", DateTime.UtcNow, "daily", "1.0"));

            // Blog-Hauptseite
            var latestBlogUpdate = await _context.Posts
                .Where(p => p.IstVeroeffentlicht)
                .OrderByDescending(p => p.ErstelltAm)
                .Select(p => p.ErstelltAm)
                .FirstOrDefaultAsync();

            urlElements.Add(CreateUrlElement(xmlns, baseUrl, "/blog",
                latestBlogUpdate != default ? latestBlogUpdate : DateTime.UtcNow,
                "daily", "0.9"));

            // Suchseite
            urlElements.Add(CreateUrlElement(xmlns, baseUrl, "/search", DateTime.UtcNow, "monthly", "0.5"));

            // Seiten
            var seiten = await _context.Seiten
                .Where(s => s.IstVeroeffentlicht)
                .ToListAsync();

            foreach (var seite in seiten)
            {
                var lastMod = seite.BearbeitetAm ?? seite.ErstelltAm;
                var changeFreq = GetChangeFrequency(seite.Template);
                var priority = GetPriority(seite.Template, seite.ImMenuAnzeigen);

                urlElements.Add(CreateUrlElement(xmlns, baseUrl, $"/seite/{seite.Slug}", lastMod, changeFreq, priority));
            }

            // Blog-Posts
            var posts = await _context.Posts
                .Where(p => p.IstVeroeffentlicht)
                .ToListAsync();

            foreach (var post in posts)
            {
                urlElements.Add(CreateUrlElement(xmlns, baseUrl, $"/blog/{post.Slug}", post.ErstelltAm, "monthly", "0.7"));
            }

            // Kategorien
            var kategorien = await _context.Kategorien
                .Where(k => k.Posts.Any(p => p.IstVeroeffentlicht))
                .ToListAsync();

            foreach (var kategorie in kategorien)
            {
                urlElements.Add(CreateUrlElement(xmlns, baseUrl, $"/blog/kategorie/{kategorie.Slug}", kategorie.ErstelltAm, "weekly", "0.6"));
            }

            var document = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(xmlns + "urlset", urlElements)
            );

            return document.ToString();
        }

        private XElement CreateUrlElement(XNamespace xmlns, string baseUrl, string location, DateTime lastMod, string changeFreq, string priority)
        {
            return new XElement(xmlns + "url",
                new XElement(xmlns + "loc", Uri.EscapeUriString($"{baseUrl}{location}")),
                new XElement(xmlns + "lastmod", lastMod.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                new XElement(xmlns + "changefreq", changeFreq),
                new XElement(xmlns + "priority", priority)
            );
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
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var document = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(xmlns + "urlset",
                    new XElement(xmlns + "url",
                        new XElement(xmlns + "loc", baseUrl + "/"),
                        new XElement(xmlns + "lastmod", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                        new XElement(xmlns + "changefreq", "daily"),
                        new XElement(xmlns + "priority", "1.0")
                    )
                )
            );

            return document.ToString();
        }
    }
}