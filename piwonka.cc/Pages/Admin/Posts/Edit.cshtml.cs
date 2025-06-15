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
		private readonly FileUploadService _fileUploadService;

		public EditModel(IDbContextFactory<ApplicationDbContext> contextFactory, FileUploadService fileUploadService)
		{
			_contextFactory = contextFactory;
			_fileUploadService = fileUploadService;
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

			// ✅ KORREKTUR 1: Alle UploadedImage-bezogenen ModelState-Fehler entfernen
			ModelState.Remove("Post.UploadedImage");

			// Zusätzlich alle Keys entfernen, die mit UploadedImage zu tun haben
			var keysToRemove = ModelState.Keys.Where(k => k.Contains("UploadedImage")).ToList();
			foreach (var key in keysToRemove)
			{
				Console.WriteLine($"Entferne ModelState Key: {key}");
				ModelState.Remove(key);
			}

			Console.WriteLine($"ModelState.IsValid nach Bereinigung: {ModelState.IsValid}");

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

			// ✅ ÄNDERUNG 1: Bild-Behandlung korrigiert
			if (Post.UploadedImage != null && Post.UploadedImage.Length > 0)
			{
				Console.WriteLine($"Neues Bild wird hochgeladen: {Post.UploadedImage.FileName}");
				// Neues Bild hochgeladen - Upload verarbeiten
				var uploadedImageUrl = await _fileUploadService.UploadImageAsync(Post.UploadedImage);
				postToUpdate.BildUrl = uploadedImageUrl;
			}
			else if (!string.IsNullOrEmpty(Post.BildUrl) && Post.BildUrl != postToUpdate.BildUrl)
			{
				Console.WriteLine($"Bild-URL wurde geändert: {Post.BildUrl}");
				// URL wurde manuell geändert
				postToUpdate.BildUrl = Post.BildUrl;
			}
			else
			{
				Console.WriteLine("Bild bleibt unverändert");
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

		private async Task LoadSelectLists()
		{
			using var context = _contextFactory.CreateDbContext();

			Console.WriteLine($"LoadSelectLists: Post.Language = {Post?.Language}");

			// Alle Kategorien mit ihren Posts laden
			var allKategorienMitPosts = await context.Kategorien
				.Include(k => k.Posts)
				.ToListAsync();

			// Nur Kategorien auswählen, die Posts in der gewünschten Sprache haben
			var kategorien = await context.Kategorien
				.Where(k => k.Language == Post.Language)
				.OrderBy(k => k.Name)
				.ToListAsync();

			Console.WriteLine($"Gefundene Kategorien für {Post.Language}: {kategorien.Count}");
			foreach (var kat in kategorien)
			{
				Console.WriteLine($"  - {kat.Name} (ID: {kat.Id})");
			}

			// Falls keine Kategorien für diese Sprache existieren, alle Kategorien anzeigen
			if (!kategorien.Any())
			{
				Console.WriteLine("Keine sprachspezifischen Kategorien gefunden - lade alle Kategorien");
				kategorien = allKategorienMitPosts.OrderBy(k => k.Name).ToList();
			}

			KategorieOptions = new SelectList(kategorien, "Id", "Name", SelectedKategorieId);

			// Sprachen laden
			var languages = new List<object>
			{
				new { Value = (int)Language.DE, Text = "Deutsch" },
				new { Value = (int)Language.EN, Text = "English" }
			};
			LanguageOptions = new SelectList(languages, "Value", "Text", (int?)Post?.Language);
		}
	}
}