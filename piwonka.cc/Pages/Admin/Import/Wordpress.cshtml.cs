using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using Piwonka.CC.Filters;


namespace Piwonka.CC.Pages.admin.Import
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class WordpressModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly HttpClient _httpClient;

        public WordpressModel(IDbContextFactory<ApplicationDbContext> contextFactory, IWebHostEnvironment environment)
        {
            _contextFactory = contextFactory;
            _environment = environment;
            _httpClient = new HttpClient();
        }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public bool Success { get; set; }

        public List<Post> ImportedPosts { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(IFormFile wpXmlFile, bool importImages = true, bool defaultCategory = true)
        {

            using (var _context = _contextFactory.CreateDbContext())
            {
                if (wpXmlFile == null || wpXmlFile.Length == 0)
                {
                    Success = false;
                    StatusMessage = "Bitte wählen Sie eine Datei aus.";
                    return Page();
                }

                try
                {
                    // XML-Datei lesen
                    XDocument doc;
                    using (var stream = wpXmlFile.OpenReadStream())
                    {
                        doc = XDocument.Load(stream);
                    }

                    // XML-Namespaces definieren
                    XNamespace wp = "http://wordpress.org/export/1.2/";
                    XNamespace content = "http://purl.org/rss/1.0/modules/content/";
                    XNamespace excerpt = "http://wordpress.org/export/1.2/excerpt/";
                    XNamespace dc = "http://purl.org/dc/elements/1.1/";

                    // Posts extrahieren
                    var items = doc.Descendants("item")
                        .Where(i => i.Elements(wp + "post_type").Any(e => e.Value == "post"));

                    ImportedPosts = new List<Post>();

                    // WordPress-Kategorien importieren oder Standard-Kategorie verwenden
                    Dictionary<string, Kategorie> kategorieMap = new Dictionary<string, Kategorie>();

                    if (!defaultCategory)
                    {
                        // WordPress-Kategorien extrahieren und in der Datenbank speichern
                        var wpCategories = doc.Descendants(wp + "category")
                            .Select(c => new
                            {
                                Slug = c.Element(wp + "category_nicename")?.Value,
                                Name = c.Element(wp + "cat_name")?.Value
                            })
                            .Where(c => !string.IsNullOrEmpty(c.Slug) && !string.IsNullOrEmpty(c.Name))
                            .ToList();

                        foreach (var wpCategory in wpCategories)
                        {
                            var existingKategorie = await _context.Kategorien
                                .FirstOrDefaultAsync(k => k.Name == wpCategory.Name);

                            if (existingKategorie == null)
                            {
                                existingKategorie = new Kategorie
                                {
                                    Name = wpCategory.Name,
                                    Beschreibung = $"Importiert von WordPress: {wpCategory.Slug}"
                                };
                                _context.Kategorien.Add(existingKategorie);
                                await _context.SaveChangesAsync();
                            }

                            kategorieMap[wpCategory.Slug] = existingKategorie;
                        }
                    }

                    // Standard-Kategorie "Allgemein" laden
                    var defaultKategorie = await _context.Kategorien
                        .FirstOrDefaultAsync(k => k.Name == "Allgemein");

                    if (defaultKategorie == null && defaultCategory)
                    {
                        defaultKategorie = new Kategorie
                        {
                            Name = "Allgemein",
                            Beschreibung = "Allgemeine Beiträge",
                            Slug = "allgemein",
                        };
                        _context.Kategorien.Add(defaultKategorie);
                        await _context.SaveChangesAsync();
                    }

                    // Posts importieren
                    foreach (var item in items)
                    {
                        var title = item.Element("title")?.Value;
                        var postContent = item.Element(content + "encoded")?.Value;
                        var pubDate = item.Element("pubDate")?.Value;
                        var isPublished = item.Element(wp + "status")?.Value == "publish";

                        // Veröffentlichungsdatum parsen
                        DateTime publishDate;
                        if (!DateTime.TryParse(pubDate, out publishDate))
                        {
                            publishDate = DateTime.Now;
                        }

                        // Kategorie bestimmen
                        Kategorie postKategorie = defaultKategorie;

                        if (!defaultCategory)
                        {
                            var categoryElement = item.Elements("category")
                                .FirstOrDefault(c => c.Attribute("domain")?.Value == "category");

                            if (categoryElement != null)
                            {
                                var categorySlug = categoryElement.Attribute("nicename")?.Value;
                                if (!string.IsNullOrEmpty(categorySlug) && kategorieMap.ContainsKey(categorySlug))
                                {
                                    postKategorie = kategorieMap[categorySlug];
                                }
                            }
                        }

                        // Bilder importieren, falls aktiviert
                        string modifiedContent = postContent;

                        if (importImages)
                        {
                            // Directory für Uploads erstellen
                            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "images", "wordpress-import");

                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            // Regulärer Ausdruck für Bilder im HTML
                            var imgRegex = new System.Text.RegularExpressions.Regex("<img[^>]*src=\"([^\"]*)\"[^>]*>");
                            var matches = imgRegex.Matches(postContent);

                            foreach (System.Text.RegularExpressions.Match match in matches)
                            {
                                try
                                {
                                    // Bild-URL extrahieren
                                    string imgUrl = match.Groups[1].Value;

                                    // Dateiname aus URL extrahieren
                                    string fileName = Path.GetFileName(new Uri(imgUrl).LocalPath);
                                    string safeFileName = Path.GetFileNameWithoutExtension(fileName).Replace(" ", "-").Replace(".", "-")
                                        + Path.GetExtension(fileName);

                                    // Neuen lokalen Pfad generieren
                                    string localPath = Path.Combine(uploadsFolder, safeFileName);
                                    string webPath = $"/uploads/images/wordpress-import/{safeFileName}";

                                    // Bild herunterladen und speichern
                                    var response = await _httpClient.GetAsync(imgUrl);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        using (var fs = new FileStream(localPath, FileMode.Create))
                                        {
                                            await response.Content.CopyToAsync(fs);
                                        }

                                        // Bild-URL im Content ersetzen
                                        modifiedContent = modifiedContent.Replace(imgUrl, webPath);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Fehler beim Bild-Download - weitermachen
                                    Console.WriteLine($"Fehler beim Herunterladen des Bildes: {ex.Message}");
                                }
                            }
                        }

                        // Featured Image (falls vorhanden)
                        string featuredImageUrl = null;
                        var mediaContent = item.Elements("enclosure")
                            .FirstOrDefault(e => e.Attribute("type")?.Value.StartsWith("image/") == true);

                        if (mediaContent != null)
                        {
                            featuredImageUrl = mediaContent.Attribute("url")?.Value;

                            // Wenn Bilder importiert werden sollen, auch das Featured Image herunterladen
                            if (importImages && !string.IsNullOrEmpty(featuredImageUrl))
                            {
                                try
                                {
                                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "images", "wordpress-import");
                                    string fileName = Path.GetFileName(new Uri(featuredImageUrl).LocalPath);
                                    string safeFileName = "featured-" + Path.GetFileNameWithoutExtension(fileName).Replace(" ", "-").Replace(".", "-")
                                        + Path.GetExtension(fileName);
                                    string localPath = Path.Combine(uploadsFolder, safeFileName);
                                    string webPath = $"/uploads/images/wordpress-import/{safeFileName}";

                                    var response = await _httpClient.GetAsync(featuredImageUrl);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        using (var fs = new FileStream(localPath, FileMode.Create))
                                        {
                                            await response.Content.CopyToAsync(fs);
                                        }

                                        featuredImageUrl = webPath;
                                    }
                                }
                                catch
                                {
                                    // Fehler ignorieren
                                }
                            }
                        }

                        // Slug erstellen
                        string slug = item.Element(wp + "post_name")?.Value;
                        if (string.IsNullOrEmpty(slug))
                        {
                            slug = SlugGenerator.GenerateSlug(title);
                        }

                        // Post erstellen und speichern
                        var post = new Post
                        {
                            Titel = title,
                            Inhalt = modifiedContent,
                            Excerpt = item.Element(excerpt + "excerpt")?.Value ?? string.Empty,
                            ErstelltAm = publishDate,
                            BildUrl = featuredImageUrl,
                            IstVeroeffentlicht = isPublished,
                            Kategorie = postKategorie,
                            Slug = slug
                        };

                        _context.Posts.Add(post);
                        ImportedPosts.Add(post);
                    }

                    await _context.SaveChangesAsync();

                    Success = true;
                    StatusMessage = $"Import erfolgreich! {ImportedPosts.Count} Posts wurden importiert.";

                    return Page();
                }
                catch (Exception ex)
                {
                    Success = false;
                    StatusMessage = $"Fehler beim Import: {ex.Message}";
                    return Page();
                }
            }
        }
    }
}
