using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.Admin.Downloads
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<DownloadApp> Apps { get; set; } = new List<DownloadApp>();

        public async Task OnGetAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            Apps = await context.DownloadApps
                .Include(a => a.Dateien)
                .OrderBy(a => a.Reihenfolge)
                .ThenBy(a => a.Name)
                .ToListAsync();
        }
    }
}
