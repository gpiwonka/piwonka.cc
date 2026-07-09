using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.ViewComponents
{
    public class MenuSeitenViewComponent : ViewComponent
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILanguageService _languageService;

        public MenuSeitenViewComponent(IDbContextFactory<ApplicationDbContext> contextFactory, ILanguageService languageService)
        {
            _contextFactory = contextFactory;
            _languageService = languageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentLanguage = await _languageService.GetCurrentLanguageAsync();

            using var _context = await _contextFactory.CreateDbContextAsync();
            var menuSeiten = await _context.Seiten
                .Where(s => s.IstVeroeffentlicht && s.ImMenuAnzeigen && s.Language == currentLanguage)
                .OrderBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();

            return View(menuSeiten);
        }
    }
}