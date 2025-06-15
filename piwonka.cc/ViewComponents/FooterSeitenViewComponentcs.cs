using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.ViewComponents
{
    public class FooterSeitenViewComponent : ViewComponent
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public FooterSeitenViewComponent(IDbContextFactory<ApplicationDbContext> context)
        {
            _contextFactory = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            using var _context = _contextFactory.CreateDbContext(); 
            var footerSeiten = await _context.Seiten
                .Where(s => s.IstVeroeffentlicht &&
                           (s.Slug == "impressum" || s.Slug == "datenschutz" || s.Slug == "kontakt"))
                .OrderBy(s => s.Titel)
                .ToListAsync();

            return View(footerSeiten);
        }
    }
}