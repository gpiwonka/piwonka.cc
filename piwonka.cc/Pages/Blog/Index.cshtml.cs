using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;

using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.Blog
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageService languageService;
        private const int PageSize = 9;

        public IndexModel(ApplicationDbContext context, ILanguageService language)
        {
            _context = context;
            languageService = language;        
        }

        public IList<Post> Posts { get; set; } = new List<Post>();
        public IList<Kategorie> Kategorien { get; set; } = new List<Kategorie>();
        public string CurrentKategorie { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public Language CurrentLanguage { get; set; } = Language.DE;

        public async Task OnGetAsync(string kategorie = null, int page = 1, string lang = null)
        {
            // Sprache aus Parameter oder Standard setzen
            CurrentLanguage = !string.IsNullOrEmpty(lang)
                ? languageService.GetLanguageFromString(lang)
                : Language.DE;

            CurrentKategorie = kategorie;
            CurrentPage = page;

            // Base Query für Posts mit Sprachfilter
            var query = _context.Posts
                .Include(p => p.Kategorie)
                .Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage);

            // Kategorie-Filter anwenden
            if (!string.IsNullOrEmpty(kategorie) && int.TryParse(kategorie, out int kategorieId))
            {
                query = query.Where(p => p.KategorieId == kategorieId);
            }

            // Gesamtanzahl für Paginierung
            var totalPosts = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)totalPosts / PageSize);

            // Posts für aktuelle Seite laden
            Posts = await query
                .OrderByDescending(p => p.ErstelltAm)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Kategorien für Filter laden (nur die mit Posts in der aktuellen Sprache)
            Kategorien = await _context.Kategorien
                .Where(k => k.Posts.Any(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage))
                .OrderBy(k => k.Name)
                .ToListAsync();
        }
    }
}