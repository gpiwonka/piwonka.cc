using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.Blog
{
    public class DetailModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;

        public DetailModel(IDbContextFactory<ApplicationDbContext> contextFactory, ILanguageService languageService, ILocalizationService localizationService)
        {
            _contextFactory = contextFactory;
            _languageService = languageService;
            _localizationService = localizationService;
        }

        public Post Post { get; set; }
        public Post PreviousPost { get; set; }
        public Post NextPost { get; set; }
        public IList<Post> RelatedPosts { get; set; } = new List<Post>();
        public IList<Post> RecentPosts { get; set; } = new List<Post>();
        public IList<Kategorie> Kategorien { get; set; } = new List<Kategorie>();
        public Language CurrentLanguage { get; set; }

        // Lokalisierte Strings für bessere Performance
        public string LocalizedSharePost { get; private set; }
        public string LocalizedPreviousPost { get; private set; }
        public string LocalizedNextPost { get; private set; }
        public string LocalizedRelatedPosts { get; private set; }
        public string LocalizedRecentPosts { get; private set; }
        public string LocalizedCategories { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id, string slug = null)
        {
            try
            {
                // Aktuelle Sprache holen
                CurrentLanguage = await _languageService.GetCurrentLanguageAsync();

                // Lokalisierte Strings laden
                await LoadLocalizedStringsAsync();

                Console.WriteLine($"=== BLOG DETAIL ===");
                Console.WriteLine($"Post ID: {id}");
                Console.WriteLine($"Current Language: {CurrentLanguage}");
                using var _context = await _contextFactory.CreateDbContextAsync();  
                // Post laden
                Post = await _context.Posts
                    .Include(p => p.Kategorie)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IstVeroeffentlicht);

                if (Post == null)
                {
                    Console.WriteLine($"Post with ID {id} not found");
                    return NotFound();
                }

                Console.WriteLine($"Post found: {Post.Titel} (Language: {Post.Language})");

                // Optional: Redirect wenn Post in falscher Sprache
                if (Post.Language != CurrentLanguage)
                {
                    Console.WriteLine($"Post language ({Post.Language}) doesn't match current language ({CurrentLanguage})");

                    // Suche nach Post in aktueller Sprache mit ähnlichem Titel oder Kategorie
                    var alternativePost = await _context.Posts
                        .Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage)
                        .OrderByDescending(p => p.ErstelltAm)
                        .FirstOrDefaultAsync();

                    if (alternativePost != null)
                    {
                        Console.WriteLine($"Redirecting to alternative post: {alternativePost.Id}");
                        return RedirectToPage("./Detail", new { id = alternativePost.Id, slug = alternativePost.Slug });
                    }

                    // Oder zur Blog-Übersicht
                    return RedirectToPage("./Index");
                }

                // Previous/Next Posts in derselben Sprache
                PreviousPost = await _context.Posts
                    .Include(p => p.Kategorie)
                    .Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage && p.Id < id)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

                NextPost = await _context.Posts
                    .Include(p => p.Kategorie)
                    .Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage && p.Id > id)
                    .OrderBy(p => p.Id)
                    .FirstOrDefaultAsync();

                // Related Posts (gleiche Kategorie, gleiche Sprache)
                if (Post.Kategorie != null)
                {
                    RelatedPosts = await _context.Posts
                        .Include(p => p.Kategorie)
                        .Where(p => p.IstVeroeffentlicht &&
                                   p.Language == CurrentLanguage &&
                                   p.Kategorie == Post.Kategorie &&
                                   p.Id != id)
                        .OrderByDescending(p => p.ErstelltAm)
                        .Take(4)
                        .ToListAsync();
                }

                // Wenn nicht genügend verwandte Posts in derselben Kategorie, fülle mit anderen Posts auf
                if (RelatedPosts.Count < 4)
                {
                    var additionalPosts = await _context.Posts
                        .Include(p => p.Kategorie)
                        .Where(p => p.IstVeroeffentlicht &&
                                   p.Language == CurrentLanguage &&
                                   p.Id != id &&
                                   !RelatedPosts.Select(rp => rp.Id).Contains(p.Id))
                        .OrderByDescending(p => p.ErstelltAm)
                        .Take(4 - RelatedPosts.Count)
                        .ToListAsync();

                    RelatedPosts = RelatedPosts.Concat(additionalPosts).ToList();
                }

                // Recent Posts in aktueller Sprache
                RecentPosts = await _context.Posts
                    .Include(p => p.Kategorie)
                    .Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage && p.Id != id)
                    .OrderByDescending(p => p.ErstelltAm)
                    .Take(5)
                    .ToListAsync();

                // Kategorien für Sidebar (nur die mit Posts in der aktuellen Sprache)
                Kategorien = await _context.Kategorien
                    .Where(k => k.Posts.Any(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage))
                    .OrderBy(k => k.Name)
                    .ToListAsync();

                Console.WriteLine($"Related posts: {RelatedPosts.Count}");
                Console.WriteLine($"Recent posts: {RecentPosts.Count}");
                Console.WriteLine($"Categories: {Kategorien.Count}");

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Blog DetailModel: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Fallback bei Fehlern
                await LoadFallbackStringsAsync();
                return NotFound();
            }
        }

        private async Task LoadLocalizedStringsAsync()
        {
            try
            {
                LocalizedSharePost = await _localizationService.GetLocalizedStringAsync("SharePost");
                LocalizedPreviousPost = await _localizationService.GetLocalizedStringAsync("PreviousPost");
                LocalizedNextPost = await _localizationService.GetLocalizedStringAsync("NextPost");
                LocalizedRelatedPosts = await _localizationService.GetLocalizedStringAsync("RelatedPosts");
                LocalizedRecentPosts = await _localizationService.GetLocalizedStringAsync("RecentPosts");
                LocalizedCategories = await _localizationService.GetLocalizedStringAsync("Categories");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading localized strings: {ex.Message}");
                await LoadFallbackStringsAsync();
            }
        }

        private async Task LoadFallbackStringsAsync()
        {
            // Fallback deutsche Strings
            LocalizedSharePost = "Teilen Sie diesen Beitrag";
            LocalizedPreviousPost = "Vorheriger Beitrag";
            LocalizedNextPost = "Nächster Beitrag";
            LocalizedRelatedPosts = "Das könnte Sie auch interessieren";
            LocalizedRecentPosts = "Neueste Beiträge";
            LocalizedCategories = "Kategorien";

            await Task.CompletedTask; // Für async consistency
        }
    }
}