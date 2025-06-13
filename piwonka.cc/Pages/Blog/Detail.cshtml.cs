// Pages/Blog/Detail.cshtml.cs - Erweitert um Sprach-Support
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

		public DetailModel(ApplicationDbContext context)
		{
			_context = context;
		}

		public Post Post { get; set; }
		public Post? PreviousPost { get; set; }
		public Post? NextPost { get; set; }
		public IList<Post> RelatedPosts { get; set; } = new List<Post>();
		public IList<Post> RecentPosts { get; set; } = new List<Post>();
		public IList<Kategorie> Kategorien { get; set; } = new List<Kategorie>();
		public string CurrentLanguage { get; set; } = "de";

		public async Task<IActionResult> OnGetAsync(int id, string? slug)
		{
			Post = await _context.Posts
				.Include(p => p.Kategorie)
				.FirstOrDefaultAsync(p => p.Id == id && p.IstVeroeffentlicht);

			if (Post == null)
			{
				return NotFound();
			}

			CurrentLanguage = Post.Language;

			// Language in Session setzen, falls vom Post abweichend
			LanguageService.SetCurrentLanguage(HttpContext, CurrentLanguage);

			// Navigation (vorheriger/nächster Post in derselben Language)
			PreviousPost = await _context.Posts
				.Where(p => p.Id < id && p.IstVeroeffentlicht && p.Language == CurrentLanguage)
				.OrderByDescending(p => p.Id)
				.FirstOrDefaultAsync();

			NextPost = await _context.Posts
				.Where(p => p.Id > id && p.IstVeroeffentlicht && p.Language == CurrentLanguage)
				.OrderBy(p => p.Id)
				.FirstOrDefaultAsync();

			// Verwandte Posts (gleiche Kategorie, gleiche Language)
			if (Post.KategorieId.HasValue)
			{
				RelatedPosts = await _context.Posts
					.Include(p => p.Kategorie)
					.Where(p => p.KategorieId == Post.KategorieId &&
							   p.Id != id &&
							   p.IstVeroeffentlicht &&
							   p.Language == CurrentLanguage)
					.Take(4)
					.ToListAsync();
			}

			// Neueste Posts in derselben Language
			RecentPosts = await _context.Posts
				.Where(p => p.IstVeroeffentlicht && p.Language == CurrentLanguage)
				.OrderByDescending(p => p.ErstelltAm)
				.Take(5)
				.ToListAsync();

			// Kategorien für die Sidebar
			Kategorien = await _context.Kategorien
				.Where(k => k.Language == CurrentLanguage)
				.OrderBy(k => k.Name)
				.ToListAsync();

			return Page();
		}
	}
}