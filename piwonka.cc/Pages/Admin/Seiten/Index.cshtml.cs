// Pages/Admin/Seiten/Index.cshtml.cs (Update)
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin.Seiten
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Seite> Seiten { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Seiten = await _context.Seiten
                .OrderBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();
        }
    }
}