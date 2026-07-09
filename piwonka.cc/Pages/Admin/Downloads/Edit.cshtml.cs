using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Downloads
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
        public DownloadApp App { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DownloadApps.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null) return NotFound();
            App = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DownloadApps.FirstOrDefaultAsync(a => a.Id == App.Id);
            if (entity == null) return NotFound();

            entity.Name = App.Name;
            entity.Beschreibung = App.Beschreibung;
            entity.Icon = App.Icon;
            entity.Reihenfolge = App.Reihenfolge;
            entity.Aktiv = App.Aktiv;
            entity.Language = App.Language;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"App '{entity.Name}' aktualisiert.";
            return RedirectToPage("./Index");
        }
    }
}
