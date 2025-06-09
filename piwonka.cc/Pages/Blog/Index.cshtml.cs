// Pages/Blog/Index.cshtml.cs (aktualisieren)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Blog
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 9; // Anzahl der Posts pro Seite

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Post> Posts { get; set; }
        public IList<Kategorie> Kategorien { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CurrentKategorie { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }


        public async Task OnGetAsync([FromQuery(Name = "page")] int? pageParam = 1, [FromQuery(Name = "kategorie")] string kategorieParam = null)
        {
            // Sicherstellen, dass die Parameter von der URL korrekt übernommen werden
            if (pageParam.HasValue)
            {
                CurrentPage = pageParam.Value;
            }

            if (!string.IsNullOrEmpty(kategorieParam))
            {
                CurrentKategorie = kategorieParam;
            }

            // Kategorien laden
            Kategorien = await _context.Kategorien.ToListAsync();

            // Query für Posts erstellen
            var postsQuery = _context.Posts
                .Include(p => p.Kategorie)
                .Where(p => p.IstVeroeffentlicht)
                .AsQueryable();

            // Nach Kategorie filtern, falls angegeben
            if (!string.IsNullOrEmpty(CurrentKategorie) && int.TryParse(CurrentKategorie, out int kategorieId))
            {
                postsQuery = postsQuery.Where(p => p.KategorieId == kategorieId);
            }

            // Gesamtzahl der Posts ermitteln für Paginierung
            var totalPosts = await postsQuery.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalPosts / (double)PageSize);

            // Stellen Sie sicher, dass die aktuelle Seite gültig ist
            if (CurrentPage < 1)
                CurrentPage = 1;
            else if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            // Posts mit Paginierung abrufen
            Posts = await postsQuery
                .OrderByDescending(p => p.ErstelltAm)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}