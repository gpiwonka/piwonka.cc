using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.admin.Posts
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory  = contextFactory;
        }

        public IList<Post> Posts { get; set; }

        public async Task OnGetAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();      
            Posts = await _context.Posts
                .OrderByDescending(p => p.ErstelltAm)
                .ToListAsync();
        }
    }
}

