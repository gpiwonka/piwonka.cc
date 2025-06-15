using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin.Seiten
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<Seite> Seiten { get; set; } = default!;

        // Property für Sprachfilter - MUSS public sein für Razor-Zugriff
        [BindProperty(SupportsGet = true)]
        public int? Language { get; set; }

        public async Task OnGetAsync(int? language = null)
        {
            // Parameter aus URL übernehmen
            if (language.HasValue)
            {
                Language = language;
            }

            using var context = _contextFactory.CreateDbContext();

            var query = context.Seiten.AsQueryable();

            // Sprachfilter anwenden falls gesetzt
            if (Language.HasValue)
            {
                var selectedLanguage = (Language)Language.Value;
                query = query.Where(s => s.Language == selectedLanguage);
            }

            Seiten = await query
                .OrderBy(s => s.Language)
                .ThenBy(s => s.Reihenfolge)
                .ThenBy(s => s.Titel)
                .ToListAsync();
        }
    }
}