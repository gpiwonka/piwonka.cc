// Pages/Admin/Seiten/Delete.cshtml.cs
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
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Seite Seite { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Seite = await _context.Seiten.FindAsync(id);

            if (Seite == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Seite = await _context.Seiten.FindAsync(Seite.Id);

            if (Seite != null)
            {
                _context.Seiten.Remove(Seite);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}