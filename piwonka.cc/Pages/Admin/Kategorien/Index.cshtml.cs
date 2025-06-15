// Pages/Admin/Kategorien/Index.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin.Kategorien
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<Kategorie> Kategorien { get; set; }

        public async Task OnGetAsync()
        {
            using var _context = _contextFactory.CreateDbContext();     
            Kategorien = await _context.Kategorien
                .Include(k => k.Posts)
                .ToListAsync();
        }
    }
}