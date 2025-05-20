using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data.Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;

namespace Piwonka.CC.Pages.admin.Posts
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Post> Posts { get; set; }

        public async Task OnGetAsync()
        {
            Posts = await _context.Posts
                .OrderByDescending(p => p.ErstelltAm)
                .ToListAsync();
        }
    }
}

