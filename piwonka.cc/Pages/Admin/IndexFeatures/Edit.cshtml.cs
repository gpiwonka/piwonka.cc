using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.IndexFeatures
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class EditModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public EditModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        [BindProperty]
        public IndexFeature Feature { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.IndexFeatures.FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null) return NotFound();
            Feature = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.IndexFeatures.FirstOrDefaultAsync(f => f.Id == Feature.Id);
            if (entity == null) return NotFound();

            entity.Titel = Feature.Titel;
            entity.Beschreibung = Feature.Beschreibung;
            entity.Link = Feature.Link;
            entity.Icon = Feature.Icon;
            entity.Reihenfolge = Feature.Reihenfolge;
            entity.Aktiv = Feature.Aktiv;
            entity.Language = Feature.Language;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Feature '{entity.Titel}' aktualisiert.";
            return RedirectToPage("./Index");
        }
    }
}
