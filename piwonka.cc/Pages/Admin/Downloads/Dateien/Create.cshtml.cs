using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Downloads.Dateien
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class CreateModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CreateModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        [BindProperty]
        public DownloadDatei Datei { get; set; } = new();

        public string AppName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int appId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var app = await context.DownloadApps.FirstOrDefaultAsync(a => a.Id == appId);
            if (app == null) return NotFound();

            AppName = app.Name;
            Datei.DownloadAppId = appId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var app = await context.DownloadApps.FirstOrDefaultAsync(a => a.Id == Datei.DownloadAppId);
            if (app == null) return NotFound();
            AppName = app.Name;

            if (!ModelState.IsValid) return Page();

            Datei.ErstelltAm = DateTime.Now;
            context.DownloadDateien.Add(Datei);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Datei '{Datei.Plattform}' zu '{app.Name}' hinzugefügt.";
            return RedirectToPage("../Index");
        }
    }
}
