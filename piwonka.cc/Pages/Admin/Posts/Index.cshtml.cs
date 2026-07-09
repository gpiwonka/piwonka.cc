using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.admin.Posts
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<Post> Posts { get; set; } = new List<Post>();

        public async Task OnGetAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            Posts = await _context.Posts
                .Include(p => p.Kategorie)
                .OrderByDescending(p => p.ErstelltAm)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostKopierenAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var source = await context.Posts
                .Include(p => p.Kategorie)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (source == null)
            {
                TempData["ErrorMessage"] = "Quell-Post nicht gefunden.";
                return RedirectToPage();
            }

            var targetLanguage = source.Language == Language.DE ? Language.EN : Language.DE;
            var langSuffix = targetLanguage == Language.EN ? "-en" : "-de";
            var newSlug = await GenerateUniqueSlugAsync(context, source.Slug, langSuffix);

            var copy = new Post
            {
                Titel = source.Titel,
                Slug = newSlug,
                Inhalt = source.Inhalt,
                Excerpt = source.Excerpt,
                MetaDescription = source.MetaDescription,
                MetaKeywords = source.MetaKeywords,
                IstVeroeffentlicht = false,
                Language = targetLanguage,
                ErstelltAm = DateTime.Now
            };

            // Kategorie wird bewusst nicht übernommen — Kategorien sind sprachgebunden,
            // der User soll im Edit die passende Kategorie der Zielsprache wählen.

            context.Posts.Add(copy);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Kopie '{copy.Titel}' als {(targetLanguage == Language.EN ? "Englisch" : "Deutsch")}-Entwurf angelegt — jetzt übersetzen.";
            return RedirectToPage("./Edit", new { id = copy.Id });
        }

        private static async Task<string> GenerateUniqueSlugAsync(ApplicationDbContext context, string sourceSlug, string langSuffix)
        {
            var baseSlug = sourceSlug.EndsWith("-de", StringComparison.OrdinalIgnoreCase) || sourceSlug.EndsWith("-en", StringComparison.OrdinalIgnoreCase)
                ? sourceSlug[..^3]
                : sourceSlug;

            var candidate = baseSlug + langSuffix;
            var suffix = 2;
            while (await context.Posts.AnyAsync(p => p.Slug == candidate))
            {
                candidate = $"{baseSlug}{langSuffix}-{suffix}";
                suffix++;
            }
            return candidate;
        }
    }
}
