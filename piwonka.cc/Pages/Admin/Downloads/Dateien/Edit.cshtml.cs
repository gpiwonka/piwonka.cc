using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Downloads.Dateien
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
        public DownloadDatei Datei { get; set; } = new();

        public string AppName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DownloadDateien
                .Include(d => d.App)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (entity == null) return NotFound();
            Datei = entity;
            AppName = entity.App?.Name ?? string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                using var ctx = await _contextFactory.CreateDbContextAsync();
                AppName = (await ctx.DownloadApps.FirstOrDefaultAsync(a => a.Id == Datei.DownloadAppId))?.Name ?? string.Empty;
                return Page();
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DownloadDateien.FirstOrDefaultAsync(d => d.Id == Datei.Id);
            if (entity == null) return NotFound();

            entity.Plattform = Datei.Plattform;
            entity.Version = Datei.Version;
            entity.Hinweis = Datei.Hinweis;
            entity.DateiPfad = Datei.DateiPfad;
            entity.Dateigroesse = Datei.Dateigroesse;
            entity.VeroeffentlichtAm = Datei.VeroeffentlichtAm;
            entity.Reihenfolge = Datei.Reihenfolge;
            entity.Aktiv = Datei.Aktiv;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Datei '{entity.Plattform}' aktualisiert.";
            return RedirectToPage("../Index");
        }
    }
}
