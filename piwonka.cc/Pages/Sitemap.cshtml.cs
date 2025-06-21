using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Services;
using System.Text;
using System.Xml;

namespace Piwonka.CC.Pages
{
    public class SitemapModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IIndexNowService _indexNowService;

        public SitemapModel(ApplicationDbContext context, IIndexNowService indexNowService)
        {
            _context = context;
            _indexNowService = indexNowService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Response.ContentType = "application/xml";

            var sitemap = await GenerateSitemapAsync();
            return Content(sitemap, "application/xml", Encoding.UTF8);
        }

        private async Task<string> GenerateSitemapAsync()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Homepage
            sb.AppendLine($"  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/</loc>");
            sb.AppendLine($"    <lastmod>{now}</lastmod>");
            sb.AppendLine($"    <changefreq>daily</changefreq>");
            sb.AppendLine($"    <priority>1.0</priority>");
            sb.AppendLine($"  </url>");

            // Blog Index
            sb.AppendLine($"  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/blog</loc>");
            sb.AppendLine($"    <lastmod>{now}</lastmod>");
            sb.AppendLine($"    <changefreq>daily</changefreq>");
            sb.AppendLine($"    <priority>0.9</priority>");
            sb.AppendLine($"  </url>");

            // Seiten
            var seiten = await _context.Seiten
                .Where(s => s.IstVeroeffentlicht)
                .OrderBy(s => s.Titel)
                .ToListAsync();

            foreach (var seite in seiten)
            {
                var lastmod = seite.BearbeitetAm?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? seite.ErstelltAm.ToString("yyyy-MM-ddTHH:mm:ssZ");
                sb.AppendLine($"  <url>");
                sb.AppendLine($"    <loc>{baseUrl}/seite/{seite.Slug}</loc>");
                sb.AppendLine($"    <lastmod>{lastmod}</lastmod>");
                sb.AppendLine($"    <changefreq>weekly</changefreq>");
                sb.AppendLine($"    <priority>0.8</priority>");
                sb.AppendLine($"  </url>");
            }

            // Blog Posts
            var posts = await _context.Posts
                .Where(p => p.IstVeroeffentlicht)
                .OrderByDescending(p => p.ErstelltAm)
                .ToListAsync();

            foreach (var post in posts)
            {
                var lastmod = post.ErstelltAm.ToString("yyyy-MM-ddTHH:mm:ssZ");
                sb.AppendLine($"  <url>");
                sb.AppendLine($"    <loc>{baseUrl}/blog/{post.Slug}</loc>");
                sb.AppendLine($"    <lastmod>{lastmod}</lastmod>");
                sb.AppendLine($"    <changefreq>monthly</changefreq>");
                sb.AppendLine($"    <priority>0.7</priority>");
                sb.AppendLine($"  </url>");
            }

            // Kategorien
            var kategorien = await _context.Kategorien
                .Where(k => k.Posts.Any(p => p.IstVeroeffentlicht))
                .ToListAsync();

            foreach (var kategorie in kategorien)
            {
                sb.AppendLine($"  <url>");
                sb.AppendLine($"    <loc>{baseUrl}/blog/kategorie/{kategorie.Slug}</loc>");
                sb.AppendLine($"    <lastmod>{now}</lastmod>");
                sb.AppendLine($"    <changefreq>weekly</changefreq>");
                sb.AppendLine($"    <priority>0.6</priority>");
                sb.AppendLine($"  </url>");
            }

            sb.AppendLine("</urlset>");

            return sb.ToString();
        }
    }
}