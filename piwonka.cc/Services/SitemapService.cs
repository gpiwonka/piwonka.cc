using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using System.Text;
using System.Xml;

namespace Piwonka.CC.Services
{
    public interface ISitemapService
    {
        Task<string> GenerateSitemapXmlAsync(string baseUrl);
        Task<List<SitemapUrl>> GetSitemapUrlsAsync();
        Task InvalidateCacheAsync();
    }

    public class SitemapService : ISitemapService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SitemapService> _logger;
        private string? _cachedSitemap;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(24);

        public SitemapService(ApplicationDbContext context, ILogger<SitemapService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateSitemapXmlAsync(string baseUrl)
        {
            try
            {
                // Cache prüfen
                if (_cachedSitemap != null &&
                    DateTime.UtcNow - _lastCacheUpdate < _cacheExpiry)
                {
                    return _cachedSitemap;
                }

                var urls = await GetSitemapUrlsAsync();
                var xml = GenerateXml(urls, baseUrl);

                // Cache aktualisieren
                _cachedSitemap = xml;
                _lastCacheUpdate = DateTime.UtcNow;

                return xml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap XML");
                return GenerateMinimalSitemap(baseUrl);
            }
        }

        public async Task<List<SitemapUrl>> GetSitemapUrlsAsync()
        {
            var urls = new List<SitemapUrl>();

            try
            {
                // Homepage
                urls.Add(new SitemapUrl
                {
                    Location = "/",
                    LastModified = DateTime.UtcNow,
                    ChangeFrequency = "daily",
                    Priority = 1.0m
                });

                // Blog-Hauptseite
                var latestBlogUpdate = await _context.Posts
                    .Where(p => p.IstVeroeffentlicht)
                    .OrderByDescending(p => p.ErstelltAm)
                    .Select(p => p.ErstelltAm)
                    .FirstOrDefaultAsync();

                urls.Add(new SitemapUrl
                {
                    Location = "/blog",
                    LastModified = latestBlogUpdate != default ? latestBlogUpdate : DateTime.UtcNow,
                    ChangeFrequency = "daily",
                    Priority = 0.9m
                });

                // Suchseite
                urls.Add(new SitemapUrl
                {
                    Location = "/search",
                    LastModified = DateTime.UtcNow,
                    ChangeFrequency = "monthly",
                    Priority = 0.5m
                });

                // Seiten
                var seiten = await _context.Seiten
                    .Where(s => s.IstVeroeffentlicht)
                    .Select(s => new
                    {
                        s.Slug,
                        s.ErstelltAm,
                        s.BearbeitetAm,
                        s.Template,
                        s.ImMenuAnzeigen,
                        s.Language
                    })
                    .ToListAsync();

                foreach (var seite in seiten)
                {
                    urls.Add(new SitemapUrl
                    {
                        Location = $"/seite/{seite.Slug}",
                        LastModified = seite.BearbeitetAm ?? seite.ErstelltAm,
                        ChangeFrequency = GetChangeFrequency(seite.Template),
                        Priority = GetPriority(seite.Template, seite.ImMenuAnzeigen),
                        Language = seite.Language.ToString().ToLower()
                    });
                }

                // Blog-Posts
                var posts = await _context.Posts
                    .Where(p => p.IstVeroeffentlicht)
                    .Select(p => new
                    {
                        p.Slug,
                        p.ErstelltAm,
                        p.Language
                    })
                    .ToListAsync();

                foreach (var post in posts)
                {
                    urls.Add(new SitemapUrl
                    {
                        Location = $"/blog/{post.Slug}",
                        LastModified = post.ErstelltAm,
                        ChangeFrequency = "monthly",
                        Priority = 0.7m,
                        Language = post.Language.ToString().ToLower()
                    });
                }

                // Kategorien
                var kategorien = await _context.Kategorien
                    .Where(k => k.Posts.Any(p => p.IstVeroeffentlicht))
                    .Select(k => new
                    {
                        k.Slug,
                        k.ErstelltAm,
                        k.Language
                    })
                    .ToListAsync();

                foreach (var kategorie in kategorien)
                {
                    urls.Add(new SitemapUrl
                    {
                        Location = $"/blog/kategorie/{kategorie.Slug}",
                        LastModified = kategorie.ErstelltAm,
                        ChangeFrequency = "weekly",
                        Priority = 0.6m,
                        Language = kategorie.Language.ToString().ToLower()
                    });
                }

                return urls.OrderByDescending(u => u.Priority)
                          .ThenByDescending(u => u.LastModified)
                          .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sitemap URLs");
                return new List<SitemapUrl>();
            }
        }

        public async Task InvalidateCacheAsync()
        {
            _cachedSitemap = null;
            _lastCacheUpdate = DateTime.MinValue;
            await Task.CompletedTask;
        }

        private string GenerateXml(List<SitemapUrl> urls, string baseUrl)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            xmlWriter.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");

            foreach (var url in urls)
            {
                xmlWriter.WriteStartElement("url");

                xmlWriter.WriteElementString("loc", $"{baseUrl.TrimEnd('/')}{url.Location}");
                xmlWriter.WriteElementString("lastmod", url.LastModified.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                xmlWriter.WriteElementString("changefreq", url.ChangeFrequency);
                xmlWriter.WriteElementString("priority", url.Priority.ToString("F1"));

                // Sprach-Alternative hinzufügen, falls vorhanden
                if (!string.IsNullOrEmpty(url.Language) && url.Language != "de")
                {
                    xmlWriter.WriteStartElement("link", "http://www.w3.org/1999/xhtml");
                    xmlWriter.WriteAttributeString("rel", "alternate");
                    xmlWriter.WriteAttributeString("hreflang", url.Language);
                    xmlWriter.WriteAttributeString("href", $"{baseUrl.TrimEnd('/')}{url.Location}");
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement(); // url
            }

            xmlWriter.WriteEndElement(); // urlset
            xmlWriter.WriteEndDocument();

            return stringWriter.ToString();
        }

        private string GenerateMinimalSitemap(string baseUrl)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
    <url>
        <loc>{baseUrl}</loc>
        <lastmod>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}</lastmod>
        <changefreq>daily</changefreq>
        <priority>1.0</priority>
    </url>
</urlset>";
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

        private decimal GetPriority(string template, bool imMenu)
        {
            if (imMenu)
            {
                return template switch
                {
                    "Kontakt" => 0.8m,
                    "Standard" => 0.7m,
                    "Vollbreite" => 0.7m,
                    _ => 0.6m
                };
            }
            return 0.5m;
        }
    }

    public class SitemapUrl
    {
        public string Location { get; set; } = string.Empty;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public string ChangeFrequency { get; set; } = "monthly";
        public decimal Priority { get; set; } = 0.5m;
        public string? Language { get; set; }
    }
}