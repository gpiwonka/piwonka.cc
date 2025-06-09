// Pages/Admin/Kategorien/Create.cshtml.cs (Debug hinzufügen)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piwonka.CC.Data;
using Piwonka.CC.Data;
using Piwonka.CC.Filters;
using Piwonka.CC.Models;
using System.Threading.Tasks;

namespace Piwonka.CC.Pages.Admin.Kategorien
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
        public Kategorie Kategorie { get; set; }

        public IActionResult OnGet()
        {
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

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState nicht gültig:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"  - {error.ErrorMessage}");
                }
                return Page();
            }

            _context.Kategorien.Add(Kategorie);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Kategorie erfolgreich erstellt: ID={Kategorie.Id}, Name={Kategorie.Name}");

            return RedirectToPage("./Index");
        }
    }
}