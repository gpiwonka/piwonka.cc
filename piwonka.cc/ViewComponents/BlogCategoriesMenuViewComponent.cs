// ViewComponents/BlogCategoriesMenuViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.ViewComponents
{
    public class BlogCategoriesMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;

        public BlogCategoriesMenuViewComponent(
            ApplicationDbContext context,
            ILanguageService languageService,
            ILocalizationService localizationService)
        {
            _context = context;
            _languageService = languageService;
            _localizationService = localizationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                // Aktuelle Sprache holen
                var currentLanguage = await _languageService.GetCurrentLanguageAsync();

                // KORREKTUR: Kategorien laden basierend auf ihrer eigenen Sprache UND Posts in der aktuellen Sprache
                var kategorien = await _context.Kategorien
                    .Where(k => k.Language == currentLanguage && // Kategorie muss in aktueller Sprache sein
                               k.Posts.Any(p => p.IstVeroeffentlicht && p.Language == currentLanguage)) // UND Posts haben
                    .Select(k => new BlogCategoryMenuItem
                    {
                        Id = k.Id,
                        Name = k.Name,
                        Slug = k.Slug, // Slug hinzufügen für Links
                        PostCount = k.Posts.Count(p => p.IstVeroeffentlicht && p.Language == currentLanguage)
                    })
                    .OrderBy(k => k.Name)
                    .ToListAsync();

                var model = new BlogCategoriesMenuViewModel
                {
                    Categories = kategorien,
                    CurrentLanguage = currentLanguage
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Fallback bei Fehlern
                Console.WriteLine($"Error in BlogCategoriesMenuViewComponent: {ex.Message}");

                return View(new BlogCategoriesMenuViewModel
                {
                    Categories = new List<BlogCategoryMenuItem>(),
                    CurrentLanguage = Language.DE
                });
            }
        }
    }

    // ViewModels für das Kategorien-Menü
    public class BlogCategoriesMenuViewModel
    {
        public List<BlogCategoryMenuItem> Categories { get; set; } = new();
        public Language CurrentLanguage { get; set; }
    }

    public class BlogCategoryMenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; // Slug hinzugefügt
        public int PostCount { get; set; }
    }
}