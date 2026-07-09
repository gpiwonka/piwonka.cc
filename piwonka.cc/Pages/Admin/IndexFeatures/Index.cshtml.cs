using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.IndexFeatures
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<IndexFeature> Features { get; set; } = new List<IndexFeature>();

        public async Task OnGetAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            Features = await context.IndexFeatures
                .OrderBy(f => f.Reihenfolge)
                .ThenByDescending(f => f.ErstelltAm)
                .ToListAsync();
        }
    }
}
