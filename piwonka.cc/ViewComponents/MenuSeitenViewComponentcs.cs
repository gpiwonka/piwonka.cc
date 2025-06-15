using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.ViewComponents
{
    public class MenuSeitenViewComponent : ViewComponent
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public MenuSeitenViewComponent(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();  
            var menuSeiten = await _context.Seiten
                .Where(s => s.IstVeroeffentlicht && s.ImMenuAnzeigen)
                .OrderBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();

            return View(menuSeiten);
        }
    }
}