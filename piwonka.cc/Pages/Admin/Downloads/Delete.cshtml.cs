using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Downloads
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
        public DownloadApp App { get; set; } = new();

        public int DateienAnzahl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DownloadApps
                .Include(a => a.Dateien)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null) return NotFound();
            App = entity;
            DateienAnzahl = entity.Dateien.Count;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DownloadApps.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null) return NotFound();

            context.DownloadApps.Remove(entity);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"App '{entity.Name}' gelöscht.";
            return RedirectToPage("./Index");
        }
    }
}
