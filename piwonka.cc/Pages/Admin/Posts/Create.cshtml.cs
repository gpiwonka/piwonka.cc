using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using System.Text.RegularExpressions;


namespace Piwonka.CC.Pages.Admin.Posts
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class CreateModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly FileUploadService _fileUploadService;
        private readonly IIndexNowService _indexNowService;        

        public CreateModel(IDbContextFactory<ApplicationDbContext> contextFactory, FileUploadService fileUploadService, IIndexNowService indexNowService)
        {
            _contextFactory = contextFactory;
            _fileUploadService = fileUploadService;
            _indexNowService = indexNowService; 
        }

        [BindProperty]
        public Post Post { get; set; } = new Post();

        public SelectList KategorieOptions { get; set; }
        public SelectList LanguageOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // KRITISCHER FIX: Explizit Inhalt aus Form holen (für TinyMCE)
            var formInhalt = Request.Form["Post.Inhalt"].ToString();
            if (string.IsNullOrEmpty(Post.Inhalt) && !string.IsNullOrEmpty(formInhalt))
            {
                Post.Inhalt = formInhalt;
                Console.WriteLine($"Blog-Inhalt aus Form übernommen: {formInhalt.Substring(0, Math.Min(100, formInhalt.Length))}...");
            }

            // DEBUG: Überprüfen was angekommen ist
            Console.WriteLine($"=== POST DEBUG ===");
            Console.WriteLine($"Post.Titel: {Post.Titel}");
            Console.WriteLine($"Post.Inhalt Length: {Post.Inhalt?.Length ?? 0}");
            Console.WriteLine($"Post.Excerpt Length: {Post.Excerpt?.Length ?? 0}");
            Console.WriteLine($"FormInhalt Length: {formInhalt?.Length ?? 0}");

            // ZUSÄTZLICHE DEBUG-INFO: Alle Form-Daten loggen
            LogFormData();

            // Slug generieren falls leer
            if (string.IsNullOrEmpty(Post.Slug))
            {
                Post.Slug = GenerateSlug(Post.Titel);
            }
            else
            {
                Post.Slug = GenerateSlug(Post.Slug);
            }

            // Prüfen, ob Slug bereits existiert
            using var _context = await _contextFactory.CreateDbContextAsync();
            if (!string.IsNullOrWhiteSpace(Post.Slug))
            {
                var existingSlug = await _context.Posts.AnyAsync(p => p.Slug == Post.Slug);
                if (existingSlug)
                {
                    ModelState.AddModelError("Post.Slug", "Dieser Slug ist bereits vergeben.");
                }
            }

            // Erweiterte Validierung
            if (string.IsNullOrWhiteSpace(Post.Titel))
            {
                ModelState.AddModelError("Post.Titel", "Der Titel ist erforderlich.");
            }

            if (string.IsNullOrWhiteSpace(Post.Inhalt))
            {
                ModelState.AddModelError("Post.Inhalt", "Der Inhalt ist erforderlich.");
            }

            if (string.IsNullOrWhiteSpace(Post.Excerpt))
            {
                ModelState.AddModelError("Post.Excerpt", "Die Kurzbeschreibung ist erforderlich.");
            }

            if (string.IsNullOrWhiteSpace(Post.Slug))
            {
                ModelState.AddModelError("Post.Slug", "Der Slug konnte nicht generiert werden.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return Page();
            }

            // Bild Upload verarbeiten
            if (Post.UploadedImage != null)
            {
                try
                {
                    Post.BildUrl = await _fileUploadService.UploadImageAsync(Post.UploadedImage);
                    Console.WriteLine($"Bild hochgeladen: {Post.BildUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Bild-Upload: {ex.Message}");
                    ModelState.AddModelError("Post.UploadedImage", "Fehler beim Hochladen des Bildes.");
                    await LoadSelectLists();
                    return Page();
                }
            }

            try
            {
                // Post mit bereinigten Daten erstellen
                var newPost = new Post
                {
                    Titel = Post.Titel.Trim(),
                    Slug = Post.Slug.Trim(),
                    Inhalt = Post.Inhalt ?? string.Empty,
                    Excerpt = Post.Excerpt?.Trim() ?? string.Empty,
                    MetaDescription = string.IsNullOrWhiteSpace(Post.MetaDescription) ? null : Post.MetaDescription.Trim(),
                    MetaKeywords = string.IsNullOrWhiteSpace(Post.MetaKeywords) ? null : Post.MetaKeywords.Trim(),
                    IstVeroeffentlicht = Post.IstVeroeffentlicht,
                    Language = Post.Language,
                    ErstelltAm = DateTime.Now,
                    BildUrl = Post.BildUrl
                };

                // Kategorie zuweisen, falls ausgewählt
                if (Post.Kategorie != null)
                {
                    var kategorie = await _context.Kategorien.FindAsync(Post.Kategorie.Id);
                    if (kategorie != null)
                    {
                        newPost.Kategorie = kategorie;
                    }
                }


                // Post erstellen
                _context.Posts.Add(newPost);
                await _context.SaveChangesAsync();
                if (newPost.IstVeroeffentlicht)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _indexNowService.NotifyPostCreatedAsync(newPost.Slug);
                        }
                        catch (Exception ex)
                        {
                            // Logging
                            Console.WriteLine($"IndexNow notification failed: {ex.Message}");
                        }
                    });
                }
                Console.WriteLine($"Post erfolgreich erstellt mit ID: {newPost.Id}");
                TempData["SuccessMessage"] = $"Post '{newPost.Titel}' wurde erfolgreich erstellt.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern des Posts: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Fehler beim Speichern des Posts.");
                await LoadSelectLists();
                return Page();
            }
        }

        private async Task LoadSelectLists()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            // Kategorien laden
            var kategorien = await _context.Kategorien.ToListAsync();
            KategorieOptions = new SelectList(kategorien, "Id", "Name");

            // Sprachen laden
            var languages = new List<object>
            {
                new { Value = (int)Language.DE, Text = "Deutsch" },
                new { Value = (int)Language.EN, Text = "English" }
            };
            LanguageOptions = new SelectList(languages, "Value", "Text");
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Replace("ä", "ae")
                      .Replace("ö", "oe")
                      .Replace("ü", "ue")
                      .Replace("Ä", "ae")
                      .Replace("Ö", "oe")
                      .Replace("Ü", "ue")
                      .Replace("ß", "ss")
                      .Replace("&", "und");

            text = text.ToLowerInvariant();
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"[\s-]+", "-");
            text = text.Trim('-');

            return text;
        }

        // ZUSÄTZLICHE DEBUG-METHODE
        private void LogFormData()
        {
            Console.WriteLine("=== FORM DATA DEBUG ===");
            foreach (var key in Request.Form.Keys)
            {
                var value = Request.Form[key].ToString();
                Console.WriteLine($"{key}: {(value.Length > 100 ? value.Substring(0, 100) + "..." : value)}");
            }
            Console.WriteLine("=== END FORM DATA ===");
        }
    }
}