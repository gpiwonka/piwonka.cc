using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.IndexFeatures
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
        public IndexFeature Feature { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.IndexFeatures.FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null) return NotFound();
            Feature = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.IndexFeatures.FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null) return NotFound();

            context.IndexFeatures.Remove(entity);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Feature '{entity.Titel}' gelöscht.";
            return RedirectToPage("./Index");
        }
    }
}
