using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.IndexFeatures
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
        public IndexFeature Feature { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            using var context = await _contextFactory.CreateDbContextAsync();
            Feature.ErstelltAm = DateTime.Now;
            context.IndexFeatures.Add(Feature);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Feature '{Feature.Titel}' wurde angelegt.";
            return RedirectToPage("./Index");
        }
    }
}
