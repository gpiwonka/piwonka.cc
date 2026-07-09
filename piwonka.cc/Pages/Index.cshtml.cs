using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages
{
    public class IndexModel : PageModel
    {
        private const int MaxBlogPosts = 5;

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILanguageService _languageService;

        public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory, ILanguageService languageService)
        {
            _contextFactory = contextFactory;
            _languageService = languageService;
        }

        public List<BlogTeaser> RecentPosts { get; set; } = new();
        public List<IndexFeature> Features { get; set; } = new();
        public AboutCard? About { get; set; }

        public async Task OnGetAsync()
        {
            var lang = await _languageService.GetCurrentLanguageAsync();

            using var context = await _contextFactory.CreateDbContextAsync();

            RecentPosts = await context.Posts
                .Where(p => p.IstVeroeffentlicht)
                .OrderByDescending(p => p.ErstelltAm)
                .Take(MaxBlogPosts)
                .Select(p => new BlogTeaser
                {
                    Id = p.Id,
                    Titel = p.Titel,
                    Slug = p.Slug,
                    Excerpt = p.Excerpt,
                    ErstelltAm = p.ErstelltAm,
                    Language = p.Language
                })
                .ToListAsync();

            Features = await context.IndexFeatures
                .Where(f => f.Aktiv && f.Language == lang)
                .OrderBy(f => f.Reihenfolge)
                .ThenByDescending(f => f.ErstelltAm)
                .ToListAsync();

            About = await context.AboutCards
                .Where(a => a.Aktiv && a.Language == lang)
                .OrderByDescending(a => a.ErstelltAm)
                .FirstOrDefaultAsync();
        }

        public class BlogTeaser
        {
            public int Id { get; set; }
            public string Titel { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string Excerpt { get; set; } = string.Empty;
            public DateTime ErstelltAm { get; set; }
            public Language Language { get; set; }
        }
    }
}
