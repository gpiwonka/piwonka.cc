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
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		private readonly ILanguageService _languageService;
		private readonly ILocalizationService _localizationService;
		private const int PageSize = 9;

		public IndexModel(IDbContextFactory<ApplicationDbContext> contextFactory, ILanguageService languageService, ILocalizationService localizationService)
		{
			_contextFactory = contextFactory;
			_languageService = languageService;
			_localizationService = localizationService;
		}

		public IList<Post> Posts { get; set; } = new List<Post>();
		public IList<Kategorie> Kategorien { get; set; } = new List<Kategorie>();
		public string CurrentKategorie { get; set; }
		public int CurrentPage { get; set; } = 1;
		public int TotalPages { get; set; }
		public Language CurrentLanguage { get; set; } = Language.DE;

		// Lokalisierte Strings für bessere Performance
		public string LocalizedBlog { get; private set; }
		public string LocalizedAllPosts { get; private set; }
		public string LocalizedNoPostsFound { get; private set; }
		public string LocalizedReadMore { get; private set; }

		public async Task OnGetAsync(string kategorie = null, int page = 1)
		{
			try
			{
				// ✅ WICHTIG: Sprache aus Session/Service holen, NICHT aus URL Parameter
				CurrentLanguage = await _languageService.GetCurrentLanguageAsync();

				// Lokalisierte Strings laden (optional für bessere Performance)
				LocalizedBlog = await _localizationService.GetLocalizedStringAsync("Blog");
				LocalizedAllPosts = await _localizationService.GetLocalizedStringAsync("AllPosts");
				LocalizedNoPostsFound = await _localizationService.GetLocalizedStringAsync("NoPostsFound");
				LocalizedReadMore = await _localizationService.GetLocalizedStringAsync("ReadMore");

				// Debug
				Console.WriteLine($"=== BLOG INDEX ===");
				Console.WriteLine($"Current Language from Service: {CurrentLanguage}");
				Console.WriteLine($"Kategorie Parameter: {kategorie}");
				Console.WriteLine($"Page Parameter: {page}");

				CurrentKategorie = kategorie;
				CurrentPage = page;
				using var _context = await _contextFactory.CreateDbContextAsync();	
				// Base Query für Posts mit Sprachfilter
				var query = _context.Posts
					.Include(p => p.Kategorie)
					.Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage);

				Console.WriteLine($"Posts query filtered by language: {CurrentLanguage}");

				// Kategorie-Filter anwenden
				if (!string.IsNullOrEmpty(kategorie) && int.TryParse(kategorie, out int kategorieId))
				{
					query = query.Where(p => p.Kategorie.Id == kategorieId);
					Console.WriteLine($"Applied category filter: {kategorieId}");
				}

				// Gesamtanzahl für Paginierung
				var totalPosts = await query.CountAsync();
				TotalPages = (int)Math.Ceiling((double)totalPosts / PageSize);

				Console.WriteLine($"Total posts found: {totalPosts}");
				Console.WriteLine($"Total pages: {TotalPages}");

				// Posts für aktuelle Seite laden
				Posts = await query
					.OrderByDescending(p => p.ErstelltAm)
					.Skip((page - 1) * PageSize)
					.Take(PageSize)
					.ToListAsync();

				Console.WriteLine($"Posts loaded for page {page}: {Posts.Count}");

				// Kategorien für Filter laden (nur die mit Posts in der aktuellen Sprache)
				Kategorien = await _context.Kategorien
					.Where(k => k.Posts.Any(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage))
					.OrderBy(k => k.Name)
					.ToListAsync();

				Console.WriteLine($"Categories found: {Kategorien.Count}");
				foreach (var kat in Kategorien)
				{
					Console.WriteLine($"  - {kat.Name} (ID: {kat.Id})");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR in Blog IndexModel: {ex.Message}");
				Console.WriteLine($"Stack Trace: {ex.StackTrace}");

				// Fallback
				CurrentLanguage = Language.DE;
				Posts = new List<Post>();
				Kategorien = new List<Kategorie>();

				// Fallback lokalisierte Strings
				LocalizedBlog = "Blog";
				LocalizedAllPosts = "Alle Beiträge";
				LocalizedNoPostsFound = "Noch keine Beiträge vorhanden.";
				LocalizedReadMore = "Weiterlesen";
			}
		}
	}
}