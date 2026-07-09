using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.About
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
        public AboutCard About { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.AboutCards.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null) return NotFound();
            About = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var formInhalt = Request.Form["About.Inhalt"].ToString();
            if (string.IsNullOrEmpty(About.Inhalt) && !string.IsNullOrEmpty(formInhalt))
            {
                About.Inhalt = formInhalt;
            }

            if (!ModelState.IsValid) return Page();

            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.AboutCards.FirstOrDefaultAsync(a => a.Id == About.Id);
            if (entity == null) return NotFound();

            entity.Titel = About.Titel;
            entity.Inhalt = About.Inhalt;
            entity.Icon = About.Icon;
            entity.Aktiv = About.Aktiv;
            entity.Language = About.Language;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"About-Karte '{entity.Titel}' aktualisiert.";
            return RedirectToPage("./Index");
        }
    }
}
