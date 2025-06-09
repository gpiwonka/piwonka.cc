using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.ViewComponents
{
    public class MenuSeitenViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public MenuSeitenViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuSeiten = await _context.Seiten
                .Where(s => s.IstVeroeffentlicht && s.ImMenuAnzeigen)
                .OrderBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();

            return View(menuSeiten);
        }
    }
}