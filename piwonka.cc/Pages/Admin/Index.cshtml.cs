// Pages/Admin/Index.cshtml.cs (erweitern)
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int AnzahlPosts { get; set; }
        public int AnzahlSeiten { get; set; }
        public int AnzahlKategorien { get; set; }
        public int AnzahlEntwürfe { get; set; }

        public async Task OnGetAsync()
        {
            AnzahlPosts = await _context.Posts.CountAsync();
            AnzahlSeiten = await _context.Seiten.CountAsync();
            AnzahlKategorien = await _context.Kategorien.CountAsync();
            AnzahlEntwürfe = await _context.Posts.CountAsync(p => !p.IstVeroeffentlicht) +
                            await _context.Seiten.CountAsync(s => !s.IstVeroeffentlicht);
        }
    }
}