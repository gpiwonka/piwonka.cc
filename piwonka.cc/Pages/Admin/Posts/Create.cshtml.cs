// Pages/Admin/Posts/Create.cshtml.cs - Erweitert um Sprach-Support
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;

using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.admin.Posts
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Post Post { get; set; } = new();

        public SelectList KategorieOptions { get; set; }
        public SelectList SprachOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadOptions();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return Page();
            }

            // Slug generieren
            Post.Slug = SlugGenerator.GenerateSlug(Post.Titel);

            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        // API-Endpoint für AJAX-Kategorien-Abfrage
        public async Task<IActionResult> OnGetKategorienByLanguageAsync(string language)
        {
            var kategorien = await _context.Kategorien
                .Where(k => k.Language == language)
                .Select(k => new { id = k.Id, name = k.Name })
                .ToListAsync();

            return new JsonResult(kategorien);
        }

        private async Task LoadOptions()
        {
            // Sprach-Optionen
            SprachOptions = new SelectList(
                LanguageService.SupportedLanguages.Select(x => new { Value = x.Key, Text = x.Value }),
                "Value", "Text", Post.Language);

            // Kategorien für die ausgewählte Language (Standard: Deutsch)
            var selectedLanguage = Post.Language ?? "de";
            var kategorien = await _context.Kategorien
                .Where(k => k.Language == selectedLanguage)
                .ToListAsync();

            KategorieOptions = new SelectList(kategorien, "Id", "Name");
        }
    }
}