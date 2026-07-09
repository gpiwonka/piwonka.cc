using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.Admin.Posts
{
	[TypeFilter(typeof(AdminAuthFilter))]
	public class EditModel : PageModel
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

		public EditModel(IDbContextFactory<ApplicationDbContext> contextFactory)
		{
			_contextFactory = contextFactory;
		}

		[BindProperty]
		public Post Post { get; set; }

		// ✅ Separate Property für Kategorie-Binding
		[BindProperty]
		public int? SelectedKategorieId { get; set; }

		public SelectList KategorieOptions { get; set; }
		public SelectList LanguageOptions { get; set; }

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Console.WriteLine($"=== OnGetAsync aufgerufen für Post ID: {id} ===");

			using var context = _contextFactory.CreateDbContext();

			Post = await context.Posts
				.Include(p => p.Kategorie)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (Post == null)
			{
				Console.WriteLine($"Post mit ID {id} nicht gefunden");
				return NotFound();
			}

			// Aktuelle Kategorie-ID setzen für Preselection
			SelectedKategorieId = Post.Kategorie?.Id;

			Console.WriteLine($"Post geladen: {Post.Titel}, Sprache: {Post.Language}, Kategorie: {Post.Kategorie?.Name ?? "Keine"}");

			await LoadSelectLists();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			Console.WriteLine("=== OnPostAsync aufgerufen ===");
			Console.WriteLine($"ModelState.IsValid vor Bereinigung: {ModelState.IsValid}");

			if (!ModelState.IsValid)
			{
				Console.WriteLine("=== ModelState-Fehler ===");
				// Debug: Zeige alle ModelState-Fehler
				foreach (var modelError in ModelState.Where(x => x.Value.Errors.Count > 0))
				{
					Console.WriteLine($"ModelState Error: {modelError.Key} - {string.Join(", ", modelError.Value.Errors.Select(e => e.ErrorMessage))}");
				}

				await LoadSelectLists();
				return Page();
			}

			Console.WriteLine("ModelState ist gültig - Post wird gespeichert");

			using var context = _contextFactory.CreateDbContext();

			var postToUpdate = await context.Posts
				.Include(p => p.Kategorie)
				.FirstOrDefaultAsync(p => p.Id == Post.Id);

			if (postToUpdate == null)
			{
				return NotFound();
			}

			// Slug generieren falls leer
			if (string.IsNullOrEmpty(Post.Slug))
			{
				Post.Slug = SlugGenerator.GenerateSlug(Post.Titel);
			}

			// Post aktualisieren
			postToUpdate.Titel = Post.Titel;
			postToUpdate.Inhalt = Post.Inhalt;
			postToUpdate.Excerpt = Post.Excerpt;
			postToUpdate.Slug = Post.Slug;
			postToUpdate.IstVeroeffentlicht = Post.IstVeroeffentlicht;
			postToUpdate.Language = Post.Language;
			postToUpdate.MetaDescription = Post.MetaDescription;
			postToUpdate.MetaKeywords = Post.MetaKeywords;

			// ✅ Kategorie über ID setzen
			if (SelectedKategorieId.HasValue)
			{
				postToUpdate.Kategorie = await context.Kategorien.FindAsync(SelectedKategorieId.Value);
				Console.WriteLine($"Kategorie gesetzt: {postToUpdate.Kategorie?.Name}");
			}
			else
			{
				postToUpdate.Kategorie = null;
				Console.WriteLine("Keine Kategorie ausgewählt");
			}

			try
			{
				await context.SaveChangesAsync();
				Console.WriteLine("Post erfolgreich gespeichert");
				TempData["SuccessMessage"] = "Post wurde erfolgreich aktualisiert.";
			}
			catch (DbUpdateConcurrencyException ex)
			{
				Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
				if (!await PostExists(Post.Id))
				{
					return NotFound();
				}
				throw;
			}

			return RedirectToPage("./Index");
		}

		private async Task<bool> PostExists(int id)
		{
			using var context = _contextFactory.CreateDbContext();
			return await context.Posts.AnyAsync(e => e.Id == id);
		}

		public async Task<IActionResult> OnGetCategoriesForLanguageAsync(int language)
		{
			using var context = _contextFactory.CreateDbContext();
			var lang = (Language)language;
			var kategorien = await context.Kategorien
				.Where(k => k.Language == lang)
				.OrderBy(k => k.Name)
				.Select(k => new { id = k.Id, name = k.Name })
				.ToListAsync();
			return new JsonResult(kategorien);
		}

		private async Task LoadSelectLists()
		{
			using var context = _contextFactory.CreateDbContext();

			var kategorien = await context.Kategorien
				.Where(k => k.Language == Post.Language)
				.OrderBy(k => k.Name)
				.ToListAsync();

			KategorieOptions = new SelectList(kategorien, "Id", "Name", SelectedKategorieId);

			var languages = new List<object>
			{
				new { Value = (int)Language.DE, Text = "Deutsch" },
				new { Value = (int)Language.EN, Text = "English" }
			};
			LanguageOptions = new SelectList(languages, "Value", "Text", (int?)Post?.Language);
		}
	}
}