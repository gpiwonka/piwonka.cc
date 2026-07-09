using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages
{
    public class DownloadsModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILanguageService _languageService;

        public DownloadsModel(IDbContextFactory<ApplicationDbContext> contextFactory, ILanguageService languageService)
        {
            _contextFactory = contextFactory;
            _languageService = languageService;
        }

        public List<DownloadApp> Apps { get; set; } = new();
        public Language CurrentLanguage { get; set; } = Language.DE;

        public async Task OnGetAsync()
        {
            CurrentLanguage = await _languageService.GetCurrentLanguageAsync();

            using var context = await _contextFactory.CreateDbContextAsync();

            Apps = await context.DownloadApps
                .Where(a => a.Aktiv && a.Language == CurrentLanguage)
                .Include(a => a.Dateien.Where(d => d.Aktiv))
                .OrderBy(a => a.Reihenfolge)
                .ThenBy(a => a.Name)
                .ToListAsync();

            // Apps ohne aktive Dateien ausblenden
            Apps = Apps.Where(a => a.Dateien.Any()).ToList();
        }
    }
}
