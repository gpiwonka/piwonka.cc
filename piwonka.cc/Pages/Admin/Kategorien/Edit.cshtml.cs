// Pages/Admin/Kategorien/Edit.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;

using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using Piwonka.CC.Services;
using Piwonka.CC.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin.Kategorien
{
    [TypeFilter(typeof(AdminAuthFilter))]
    public class EditModel : PageModel
    {
        IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILanguageService _languageService;

        public EditModel(IDbContextFactory<ApplicationDbContext> contextFactory, ILanguageService languageService)
        {
            _contextFactory = contextFactory;
            _languageService = languageService;
        }

        [BindProperty]
        public KategorieEditViewModel KategorieViewModel { get; set; }

        public SelectList LanguageOptions { get; set; }
        public Language CurrentLanguage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            // Aktuelle Sprache holen
            CurrentLanguage = await _languageService.GetCurrentLanguageAsync();

            var kategorie = await _context.Kategorien.FindAsync(id);

            if (kategorie == null)
            {
                return NotFound();
            }

            // Entity zu ViewModel konvertieren
            KategorieViewModel = new KategorieEditViewModel
            {
                Id = kategorie.Id,
                Name = kategorie.Name,
                Beschreibung = kategorie.Beschreibung,
                Language = kategorie.Language 
            };

            await LoadLanguageOptions();

            Console.WriteLine($"Kategorie geladen - ID: {KategorieViewModel.Id}, Name: {KategorieViewModel.Name}, Sprache: {KategorieViewModel.Language}");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Debug: Form-Daten ausgeben
            Console.WriteLine("=== KATEGORIE EDIT FORM-DATEN ===");
            foreach (var item in Request.Form)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
            Console.WriteLine("==================================");

            // Explizit Beschreibung aus Form holen
            var formBeschreibung = Request.Form["KategorieViewModel.Beschreibung"].ToString();
            if (string.IsNullOrEmpty(KategorieViewModel.Beschreibung) && !string.IsNullOrEmpty(formBeschreibung))
            {
                KategorieViewModel.Beschreibung = formBeschreibung;
                Console.WriteLine("Beschreibung aus Form-Daten übernommen");
            }

            Console.WriteLine($"ViewModel nach Verarbeitung - Name: {KategorieViewModel.Name}, Beschreibung: {KategorieViewModel.Beschreibung}, Sprache: {KategorieViewModel.Language}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState nicht gültig:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"  - {error.ErrorMessage}");
                }

                await LoadLanguageOptions();
                return Page();
            }

            using var _context = await _contextFactory.CreateDbContextAsync();      
            // Existierende Entity aus der Datenbank laden
            var existingKategorie = await _context.Kategorien.FindAsync(KategorieViewModel.Id);
            if (existingKategorie == null)
            {
                return NotFound();
            }

            // ViewModel-Daten auf Entity übertragen
            existingKategorie.Name = KategorieViewModel.Name;
            existingKategorie.Beschreibung = KategorieViewModel.Beschreibung;
            existingKategorie.Language = KategorieViewModel.Language;

            _context.Entry(existingKategorie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Kategorie erfolgreich aktualisiert!");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KategorieExists(KategorieViewModel.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool KategorieExists(int id)
        {
            using var _context = _contextFactory.CreateDbContext();     
            return _context.Kategorien.Any(e => e.Id == id);
        }

        private async Task LoadLanguageOptions()
        {
            var languages = new List<object>
            {
                new { Value = (int)Language.DE, Text = "Deutsch" },
                new { Value = (int)Language.EN, Text = "English" }
            };

            LanguageOptions = new SelectList(languages, "Value", "Text", (int)KategorieViewModel?.Language);
        }
    }
}