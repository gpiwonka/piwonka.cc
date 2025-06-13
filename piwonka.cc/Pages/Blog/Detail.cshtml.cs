using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;

using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.Blog
{
	public class DetailModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		private readonly ILanguageService _languageService;

		public DetailModel(ApplicationDbContext context,ILanguageService languageService)
		{
			_context = context;
			_languageService = languageService;	
		}

		public Post Post { get; set; }
		public Post PreviousPost { get; set; }
		public Post NextPost { get; set; }
		public IList<Post> RelatedPosts { get; set; } = new List<Post>();
		public IList<Kategorie> Kategorien { get; set; } = new List<Kategorie>();
		public IList<Post> RecentPosts { get; set; } = new List<Post>();
		public Language CurrentLanguage { get; set; } = Language.DE;

		public async Task<IActionResult> OnGetAsync(int id, string slug = null, String lang = null)
		{
			// Sprache aus Parameter oder Standard setzen
			CurrentLanguage = !string.IsNullOrEmpty(lang)
				? _languageService.GetLanguageFromString(lang)
				: Language.DE;

			// Post laden
			Post = await _context.Posts
				.Include(p => p.Kategorie)
				.Where(p => p.Id == id && p.IstVeroeffentlicht && p.Language == CurrentLanguage)
				.FirstOrDefaultAsync();

			if (Post == null)
			{
				return NotFound();
			}

			// SEO: Redirect wenn Slug nicht korrekt ist
			if (!string.IsNullOrEmpty(Post.Slug) && slug != Post.Slug)
			{
				var languageCode = LanguageService.GetLanguageCodeStatic(CurrentLanguage);
				return RedirectToPage("./Detail", new { id = Post.Id, slug = Post.Slug, lang = languageCode });
			}

			// Vorherigen und nächsten Post laden
			await LoadNavigationPosts(id);

			// Verwandte Posts laden (gleiche Kategorie)
			await LoadRelatedPosts();

			// Kategorien für Sidebar laden
			await LoadKategorien();

			// Neueste Posts für Sidebar laden
			await LoadRecentPosts();

			return Page();
		}

		private async Task LoadNavigationPosts(int currentPostId)
		{
			PreviousPost = await _context.Posts
				.Where(p => p.Id < currentPostId && p.IstVeroeffentlicht && p.Language == CurrentLanguage)
				.OrderByDescending(p => p.Id)
				.FirstOrDefaultAsync();

			NextPost = await _context.Posts
				.Where(p => p.Id > currentPostId && p.IstVeroeffentlicht && p.Language == CurrentLanguage)
				.OrderBy(p => p.Id)
				.FirstOrDefaultAsync();
		}

		private async Task LoadRelatedPosts()
		{
			if (Post.KategorieId.HasValue)
			{
				RelatedPosts = await _context.Posts
					.Where(p => p.KategorieId == Post.KategorieId &&
							   p.Id != Post.Id &&
							   p.IstVeroeffentlicht &&
							   p.Language == CurrentLanguage)
					.OrderByDescending(p => p.ErstelltAm)
					.Take(4)
					.ToListAsync();
			}
		}

		private async Task LoadKategorien()
		{
			Kategorien = await _context.Kategorien
				.Where(k => k.Posts.Any(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage))
				.OrderBy(k => k.Name)
				.ToListAsync();
		}

		private async Task LoadRecentPosts()
		{
			RecentPosts = await _context.Posts
				.Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage && p.Id != Post.Id)
				.OrderByDescending(p => p.ErstelltAm)
				.Take(5)
				.ToListAsync();
		}
	}
}