using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.About
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
        public AboutCard About { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.AboutCards.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null) return NotFound();
            About = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.AboutCards.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null) return NotFound();

            context.AboutCards.Remove(entity);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"About-Karte '{entity.Titel}' gelöscht.";
            return RedirectToPage("./Index");
        }
    }
}
