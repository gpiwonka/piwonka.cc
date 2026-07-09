using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Piwonka.CC.Pages.Admin.Seiten
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<Seite> Seiten { get; set; } = default!;

        // Property für Sprachfilter - MUSS public sein für Razor-Zugriff
        [BindProperty(SupportsGet = true)]
        public int? Language { get; set; }

        public async Task OnGetAsync(int? language = null)
        {
            if (language.HasValue)
            {
                Language = language;
            }

            using var context = _contextFactory.CreateDbContext();

            var query = context.Seiten.AsQueryable();

            if (Language.HasValue)
            {
                var selectedLanguage = (Models.Language)Language.Value;
                query = query.Where(s => s.Language == selectedLanguage);
            }

            Seiten = await query
                .OrderBy(s => s.Language)
                .ThenBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostKopierenAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var source = await context.Seiten.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (source == null)
            {
                TempData["ErrorMessage"] = "Quell-Seite nicht gefunden.";
                return RedirectToPage();
            }

            var targetLanguage = source.Language == Models.Language.DE ? Models.Language.EN : Models.Language.DE;
            var langSuffix = targetLanguage == Models.Language.EN ? "-en" : "-de";
            var newSlug = await GenerateUniqueSlugAsync(context, source.Slug, langSuffix);

            var copy = new Seite
            {
                Titel = source.Titel,
                Slug = newSlug,
                Inhalt = source.Inhalt,
                MetaDescription = source.MetaDescription,
                MetaKeywords = source.MetaKeywords,
                JsonLdTyp = source.JsonLdTyp,
                IstVeroeffentlicht = false,
                ImMenuAnzeigen = false,
                Reihenfolge = source.Reihenfolge,
                Template = source.Template,
                Language = targetLanguage,
                ParentId = source.ParentId,
                MenuGruppe = source.MenuGruppe,
                MenuTitel = source.MenuTitel,
                IstMenuGruppe = source.IstMenuGruppe,
                ErstelltAm = DateTime.Now,
                BearbeitetAm = null
            };

            context.Seiten.Add(copy);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Kopie '{copy.Titel}' als {(targetLanguage == Models.Language.EN ? "Englisch" : "Deutsch")}-Entwurf angelegt — jetzt übersetzen.";
            return RedirectToPage("./Edit", new { id = copy.Id });
        }

        private static async Task<string> GenerateUniqueSlugAsync(ApplicationDbContext context, string sourceSlug, string langSuffix)
        {
            var baseSlug = sourceSlug.EndsWith("-de", StringComparison.OrdinalIgnoreCase) || sourceSlug.EndsWith("-en", StringComparison.OrdinalIgnoreCase)
                ? sourceSlug[..^3]
                : sourceSlug;

            var candidate = baseSlug + langSuffix;
            var suffix = 2;
            while (await context.Seiten.AnyAsync(s => s.Slug == candidate))
            {
                candidate = $"{baseSlug}{langSuffix}-{suffix}";
                suffix++;
            }
            return candidate;
        }
    }
}
