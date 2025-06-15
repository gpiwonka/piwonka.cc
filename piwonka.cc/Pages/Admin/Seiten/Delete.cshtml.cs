using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Filters;

namespace Piwonka.CC.Pages.Admin.Seiten
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
        public Seite Seite { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            Seite = await context.Seiten.FindAsync(id);

            if (Seite == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            Seite = await context.Seiten.FindAsync(Seite.Id);

            if (Seite != null)
            {
                context.Seiten.Remove(Seite);
                await context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}