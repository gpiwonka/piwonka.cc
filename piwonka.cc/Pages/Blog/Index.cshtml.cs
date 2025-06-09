// Pages/Blog/Index.cshtml.cs - Erweitert um Sprach-Support
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

		public IndexModel(ApplicationDbContext context)
		{
			_context = context;
		}

		public IList<Post> Posts { get; set; } = new List<Post>();
		public IList<Kategorie> Kategorien { get; set; } = new List<Kategorie>();
		public int CurrentPage { get; set; } = 1;
		public int TotalPages { get; set; }
		public string? CurrentKategorie { get; set; }
		public string CurrentLanguage { get; set; } = "de";

		[BindProperty(SupportsGet = true)]
		public string? Lang { get; set; }

		public async Task OnGetAsync(int page = 1, string? kategorie = null)
		{
			// Aktuelle Sprache bestimmen
			CurrentLanguage = LanguageService.GetCurrentLanguage(HttpContext);

			// Sprache aus URL-Parameter berücksichtigen
			if (!string.IsNullOrEmpty(Lang) && LanguageService.SupportedLanguages.ContainsKey(Lang))
			{
				CurrentLanguage = Lang;
				LanguageService.SetCurrentLanguage(HttpContext, Lang);
			}

			CurrentPage = page;
			CurrentKategorie = kategorie;
			const int pageSize = 6;

			// Query für Posts in der aktuellen Sprache
			var query = _context.Posts
				.Include(p => p.Kategorie)
				.Where(p => p.IstVeroeffentlicht && p.Sprache == CurrentLanguage);

			// Kategorie-Filter
			if (!string.IsNullOrEmpty(kategorie) && int.TryParse(kategorie, out int kategorieId))
			{
				query = query.Where(p => p.KategorieId == kategorieId);
			}

			// Gesamtanzahl für Paginierung
			var totalPosts = await query.CountAsync();
			TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

			// Posts für aktuelle Seite
			Posts = await query
				.OrderByDescending(p => p.ErstelltAm)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Kategorien für die aktuelle Sprache
			Kategorien = await _context.Kategorien
				.Where(k => k.Sprache == CurrentLanguage)
				.OrderBy(k => k.Name)
				.ToListAsync();
		}
	}
}