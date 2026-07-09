using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.About
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<AboutCard> AboutCards { get; set; } = new List<AboutCard>();

        public async Task OnGetAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            AboutCards = await context.AboutCards
                .OrderBy(a => a.Language)
                .ThenByDescending(a => a.ErstelltAm)
                .ToListAsync();
        }
    }
}
