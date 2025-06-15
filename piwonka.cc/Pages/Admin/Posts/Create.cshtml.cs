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
	public class CreateModel : PageModel
	{
		IDbContextFactory<ApplicationDbContext> _contextFactory;

        private readonly FileUploadService _fileUploadService;

		public CreateModel(IDbContextFactory<ApplicationDbContext> contextFactory, FileUploadService fileUploadService)
		{
			_contextFactory = contextFactory;
			_fileUploadService = fileUploadService;
		}

		[BindProperty]
		public Post Post { get; set; } = new Post();

		public SelectList KategorieOptions { get; set; }
		public SelectList LanguageOptions { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			await LoadSelectLists();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				await LoadSelectLists();
				return Page();
			}

			// Slug generieren falls leer
			if (string.IsNullOrEmpty(Post.Slug))
			{
				Post.Slug = SlugGenerator.GenerateSlug(Post.Titel);
			}

			// Bild Upload verarbeiten
			if (Post.UploadedImage != null)
			{
				Post.BildUrl = await _fileUploadService.UploadImageAsync(Post.UploadedImage);
			}
			using var _context = await _contextFactory.CreateDbContextAsync();		
            // Post erstellen
            _context.Posts.Add(Post);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Post wurde erfolgreich erstellt.";
			return RedirectToPage("./Index");
		}

		private async Task LoadSelectLists()
		{
			using var _context = await _contextFactory.CreateDbContextAsync();		
            // Kategorien laden
            var kategorien = await _context.Kategorien.ToListAsync();
			KategorieOptions = new SelectList(kategorien, "Id", "Name");

			// Sprachen laden
			var languages = new List<object>
			{
				new { Value = (int)Language.DE, Text = "Deutsch" },
				new { Value = (int)Language.EN, Text = "English" }
			};
			LanguageOptions = new SelectList(languages, "Value", "Text");
		}
	}
}