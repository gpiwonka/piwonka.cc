using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Downloads.Dateien
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class DeleteModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DeleteModel(IDbContextFactory<ApplicationDbContext> contextFactory)
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

        public async Task<IActionResult> OnPostAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DownloadDateien.FirstOrDefaultAsync(d => d.Id == id);
            if (entity == null) return NotFound();

            context.DownloadDateien.Remove(entity);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Datei '{entity.Plattform}' gelöscht.";
            return RedirectToPage("../Index");
        }
    }
}
