using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.About
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
        public AboutCard About { get; set; } = new();

        public void OnGet()
        {
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
            About.ErstelltAm = DateTime.Now;
            context.AboutCards.Add(About);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"About-Karte '{About.Titel}' wurde angelegt.";
            return RedirectToPage("./Index");
        }
    }
}
