using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;

using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using Piwonka.CC.Services;

namespace Piwonka.CC.Pages.Admin.Kategorien
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class CreateModel : PageModel
    {
        IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CreateModel(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        [BindProperty]
        public Kategorie Kategorie { get; set; } = new Kategorie();

        public IActionResult OnGet()
        {
            // Standard-Sprache setzen
            Kategorie.Language = Language.DE;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Debug: Form-Daten ausgeben
            Console.WriteLine("=== KATEGORIE CREATE FORM-DATEN ===");
            foreach (var item in Request.Form)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
            Console.WriteLine("====================================");
            Console.WriteLine($"Kategorie.Name: '{Kategorie?.Name}'");
            Console.WriteLine($"Kategorie.Beschreibung: '{Kategorie?.Beschreibung}'");
            Console.WriteLine($"Kategorie.Language: '{Kategorie?.Language}'");

            // Datum setzen
            Kategorie.ErstelltAm = DateTime.Now;

            // Slug generieren, falls nicht gesetzt
            if (string.IsNullOrEmpty(Kategorie.Slug))
            {
                Kategorie.Slug = SlugGenerator.GenerateSlug(Kategorie.Name);
            }

            Console.WriteLine($"Generierter Slug: '{Kategorie.Slug}'");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState nicht gültig:");
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"  - {modelState.Key}: {error.ErrorMessage}");
                    }
                }

                return Page();
            }

            try
            {
                using var _context = await _contextFactory.CreateDbContextAsync();
                _context.Kategorien.Add(Kategorie);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Kategorie erfolgreich erstellt: ID={Kategorie.Id}, Name={Kategorie.Name}, Slug={Kategorie.Slug}, Language={Kategorie.Language}");

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
                ModelState.AddModelError("", "Fehler beim Speichern der Kategorie: " + ex.Message);

                return Page();
            }
        }
    }
}