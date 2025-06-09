// Pages/Seite.cshtml.cs (Update für neue Property-Namen)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages
{
    public class SeiteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SeiteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Seite Seite { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var seite = await _context.Seiten
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (seite == null || !seite.IstVeroeffentlicht)
            {
                return NotFound();
            }

            Seite = seite;

            // SEO Meta-Daten setzen
            ViewData["Title"] = seite.Titel;
            if (!string.IsNullOrEmpty(seite.MetaDescription))
            {
                ViewData["MetaDescription"] = seite.MetaDescription;
            }
            if (!string.IsNullOrEmpty(seite.MetaKeywords))
            {
                ViewData["MetaKeywords"] = seite.MetaKeywords;
            }

            return Page();
        }
    }
}